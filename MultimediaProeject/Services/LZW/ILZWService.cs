using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultimediaProject.Services.LZW
{
    public interface ILZWService
    {
        public List<string> EncodeLZW(IFormFile input);
        public List<string> DecryptLZW(IFormFile input);
    }
}
