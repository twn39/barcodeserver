namespace barcodeserver.Controllers

open ZXing
open System.IO
open ZXing.QrCode
open ZXing.QrCode.Internal
open System.Drawing.Imaging
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open System.ComponentModel.DataAnnotations
open System.Runtime.InteropServices

type FormatData = {
    ContentType: string;
    ImageFormat: ImageFormat;
}


[<ApiController>]
[<Route("/")>]
type BarCodeController (logger : ILogger<BarCodeController>) =
    inherit ControllerBase()
    
    let formatMap = Map.empty.Add("png", {ContentType = "images/png"; ImageFormat = ImageFormat.Png})
                        .Add("jpg", {ContentType = "images/jpeg"; ImageFormat = ImageFormat.Jpeg})
                        .Add("gif", {ContentType = "images/gif"; ImageFormat = ImageFormat.Gif})
                        .Add("bmp", {ContentType = "images/bmp"; ImageFormat = ImageFormat.Bmp})
    
    [<HttpGet>]
    member _.Home() =
        OkObjectResult({| Title = "BarCode service"; Version = "1.0.0" |})
    
    [<HttpGet>]
    [<Route("/qrcode")>]
    [<ResponseCache(VaryByQueryKeys = [|"w"; "h"; "m"; "f"; "d"|], Duration = 86400)>]
    member _.GetQrCode([<FromQuery(Name="w");Optional;DefaultParameterValue(400)>] width: int,
                       [<FromQuery(Name="h");Optional;DefaultParameterValue(400)>] height: int,
                       [<FromQuery(Name="m");Optional;DefaultParameterValue(1)>] margin: int,
                       [<FromQuery(Name="f");Optional;DefaultParameterValue("png")>] format: string,
                       [<FromQuery(Name="d"); Required>] data: string) =
        let qrCodeWriter = BarcodeWriter(Format = BarcodeFormat.QR_CODE,
                                         Options = QrCodeEncodingOptions(ErrorCorrection = ErrorCorrectionLevel.Q,
                                                                         CharacterSet = "UTF-8",
                                                                         Width=width,
                                                                         Height=height,
                                                                         Margin=margin))
        use image = qrCodeWriter.Write(data)
        use ms = new MemoryStream()
        image.Save(ms, formatMap.[format].ImageFormat)
        let file = FileContentResult(ms.ToArray(), formatMap.[format].ContentType)
        file.FileDownloadName <- $"qrcode.{format}"
        file
        //let base64 = Convert.ToBase64String(ms.ToArray())
        //$"data:image/png;base64,{base64}"
        
