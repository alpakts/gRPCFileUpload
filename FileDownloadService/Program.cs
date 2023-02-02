using FileUploadgRPCServiceClient;
using Grpc.Core;
using Grpc.Net.Client;

var channel = GrpcChannel.ForAddress("https://localhost:7009");
var client = new FileService.FileServiceClient(channel);
string file = "Postman-win64-Setup.exe";
string downloadPath = Directory.GetCurrentDirectory();
var fileInfo = new FileUploadgRPCServiceClient.FileInfo() { FileExtension = Path.GetExtension(file), FileName = Path.GetFileNameWithoutExtension(file) };
var response=client.DownloadFile(fileInfo);
FileStream fileStream = null;
int count = 0;
decimal chunksize = 0;
CancellationTokenSource tokensoruce = new CancellationTokenSource();
while (await response.ResponseStream.MoveNext(tokensoruce.Token))
{
	if (count++==0)
	{
        fileStream=new FileStream(file, FileMode.CreateNew, FileAccess.Write);
		fileStream.SetLength(response.ResponseStream.Current.FileSize);
	}
	var buffer=response.ResponseStream.Current.Buffer.ToByteArray();
	await fileStream.WriteAsync(buffer, 0, response.ResponseStream.Current.ReadedBytes);
    Console.WriteLine($"Downloading: {Math.Round(((chunksize += response.ResponseStream.Current.ReadedBytes) * 100) / response.ResponseStream.Current.FileSize)}½");

}



