using FileUploadgRPCService;
using Google.Protobuf;
using Grpc.Core;

namespace gRPCService.Services
{
    public class FileFuncs:FileService.FileServiceBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileFuncs> _logger;
        public FileFuncs(IWebHostEnvironment env, ILogger<FileFuncs> logger)
        {
            _env = env;
            _logger = logger;
        }

        public override async Task DownloadFile(FileUploadgRPCService.FileInfo request, IServerStreamWriter<ByteContent> responseStream, ServerCallContext context)
        {
            var path = Path.Combine(_env.WebRootPath ,"uploadFiles" , request.FileName + request.FileExtension);
            try
            {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var buffer = new byte[2048];
                decimal chunksize = 0;
                var fileInfo = new FileUploadgRPCService.FileInfo() { FileExtension = request.FileExtension, FileName = request.FileName };
                var byteContent = new ByteContent() { FileInfo = fileInfo, FileSize = fileStream.Length, ReadedBytes = 0 };
                while ((byteContent.ReadedBytes = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    byteContent.Buffer = ByteString.CopyFrom(buffer);
                    await responseStream.WriteAsync(byteContent);
                    Console.WriteLine($"Downloading: {Math.Round(((chunksize += byteContent.ReadedBytes) * 100) / byteContent.FileSize)}½");
                }
               
            }
            catch (Exception ex)
            {

                _logger.LogInformation("An Error Occured!! {0}",ex);
            }
            
        }
        public override async Task<FileUploadgRPCService.FileInfo> UploadFile(IAsyncStreamReader<ByteContent> requestStream, ServerCallContext context)
        {
            if (!Directory.Exists(_env.WebRootPath))
            {
                
            }
            int count = 0;
            decimal chunksize = 0;
            FileStream fileStream = null;
            
            var filePath = Path.Combine(_env.WebRootPath,"Uploadfiles");
            Directory.CreateDirectory(filePath);
            try
            {
                
                
                while (await requestStream.MoveNext())
                {
                    if (count++ == 0)
                    {
                        fileStream = new FileStream(Path.Combine(filePath , requestStream.Current.FileInfo.FileName+requestStream.Current.FileInfo.FileExtension), FileMode.CreateNew, FileAccess.Write);
                        fileStream.SetLength(requestStream.Current.FileSize);
                    }
                    var buffer = requestStream.Current.Buffer.ToByteArray();
                    await fileStream.WriteAsync(buffer, 0, buffer.Length);
                    Console.WriteLine($"Downloading: {Math.Round((chunksize += requestStream.Current.ReadedBytes) * 100 / requestStream.Current.FileSize)}½");
                }
                fileStream.Close();
                fileStream.Dispose();
            }
            catch (Exception exception)
            {
                _logger.LogError("An Error Occured \n {}", exception);
            }
            
            return new() { FileExtension= Path.GetExtension(fileStream.Name), FileName= fileStream.Name };
        }
    }
}