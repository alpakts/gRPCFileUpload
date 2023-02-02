
using FileUploadgRPCServiceClient;
using Google.Protobuf;
using Grpc.Net.Client;

var channel = GrpcChannel.ForAddress("https://localhost:7009");
var client = new FileService.FileServiceClient(channel);
string file = @"C:/Users/Alper/Desktop/Postman-win64-Setup.exe";
using FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
var fileInfo = new FileUploadgRPCServiceClient.FileInfo() { FileExtension = Path.GetExtension(file), FileName = Path.GetFileNameWithoutExtension(file) };
var content = new ByteContent() { FileSize = fileStream.Length, FileInfo = fileInfo, ReadedBytes = 0 };
var upload = client.UploadFile();
var buffer = new byte[2048];
while ((content.ReadedBytes=await fileStream.ReadAsync(buffer,0,buffer.Length))>0)
{
    content.Buffer = ByteString.CopyFrom(buffer);
    await upload.RequestStream.WriteAsync(content);
}
await upload.RequestStream.CompleteAsync();
fileStream.Close();