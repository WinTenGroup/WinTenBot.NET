using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Telegram.Bot.Framework.Abstractions;
using WinTenDev.Zizi.Services;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Host.Handlers.Commands.Additional
{
    public class CheckResiCommand : CommandBase
    {
        private readonly TelegramService _telegramService;
        private readonly CekResiService _cekResiService;

        public CheckResiCommand(
            CekResiService cekResiService,
            TelegramService telegramService
        )
        {
            _cekResiService = cekResiService;
            _telegramService = telegramService;
        }

        public override async Task HandleAsync(IUpdateContext context, UpdateDelegate next, string[] args, CancellationToken cancellationToken)
        {
            await _telegramService.AddUpdateContext(context);

            var resi = _telegramService.MessageTextParts.ValueOfIndex(1);

            await _telegramService.SendTextAsync("🔄 Memeriksa nomor Resi");

            if (resi.IsNullOrEmpty())
            {
                await _telegramService.EditAsync("⚠️ Silakan sertakan nomor resi yang mau di cek.");
                return;
            }

#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                Log.Debug("Running check in background");
                await CheckResi(resi);
            });
        }

        private async Task CheckResi(string resi)
        {
            await _telegramService.EditAsync("🔍 Sedang memeriksa nomor Resi");
            var runCekResi = await _cekResiService.GetResi(resi);
            Log.Debug("Check Results: {0}", runCekResi.ToJson(true));

            if (runCekResi.Result == null)
            {
                await _telegramService.EditAsync($"Sepertinya resi tidak ditemukan, silakan periksa kembali. " +
                                                 $"\nNo Resi: <code>{resi}</code>");
                return;
            }

            var result = runCekResi.Result;
            var summary = result.Summary;
            var manifests = result.Manifest;

            var manifestStr = new StringBuilder();
            foreach (var manifest in manifests)
            {
                manifestStr.AppendLine($"⏰ {manifest.ManifestDate:dd MMM yyyy} {manifest.ManifestTime:HH:mm zzz}");
                manifestStr.AppendLine($"└ {manifest.ManifestCode} - {manifest.ManifestDescription}");
                manifestStr.AppendLine();
            }

            var send =
                $"Status: {summary.Status}" +
                $"\nKurir: {summary.CourierCode} - {summary.CourierName} {summary.ServiceCode}" +
                $"\nNoResi: {summary.WaybillNumber}" +
                $"\nPengirim: {summary.ShipperName}" +
                $"\nOrigin: {summary.Origin}" +
                $"\nPenerima: {summary.ReceiverName}" +
                $"\nTujuan: {summary.Destination}" +
                $"\n\nDetail" +
                $"\n{manifestStr.ToString().Trim()}";


            await _telegramService.EditAsync(send);
        }
    }
}