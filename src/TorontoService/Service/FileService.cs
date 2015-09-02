using BahamutCommon;

using BahamutService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace TorontoService
{
    public class FileService : IAccountSessionData
    {
        public AccountSessionData UserSessionData { get; set; }
    }

    public static class FileServiceExtensions
    {
        public static IBahamutServiceBuilder UseFileService(this IBahamutServiceBuilder builder, params object[] args)
        {
            return builder.UseService<FileService>(args);
        }

        public static FileService GetAuthenticationService(this IBahamutServiceBuilder builder)
        {
            return builder.GetService<FileService>();
        }
    }
}
