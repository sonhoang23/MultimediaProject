using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultimediaProject.Services.LZW
{
    public class LZWService : ILZWService
    {
        public List<string> DecryptLZW(IFormFile fileInput)
        {
            if (fileInput != null)
            {
                var result = new StringBuilder();
                using (var reader = new StreamReader(fileInput.OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        result.AppendLine(reader.ReadLine());
                }
                String inputString = result.ToString();
                List<string> output = new List<string>();
                String entry = "";
                String w = ""; //bufer
                String k = "";  //characterInput
                char[] b = new char[inputString.Length]; //mảng ký tự lưu các kí tự trong inputString
                bool characterFirst = false;
                int numberOfCode = 256;
                Dictionary<String, String> dictionaryCharacter = new Dictionary<string, string>();
                /*----------------------------------------------------*/
                using (StringReader sr = new StringReader(inputString))
                {
                    Encoding ascii = Encoding.ASCII;
                    sr.Read(b, 0, inputString.Length);  // Add các ký tự vào mảng ký tự b
                    /*------------------------------------------------*/
                    for (int index = 0; index < b.Length; index++)
                    {

                        if (b[index] == '\r' && b[index + 1] == '\n')
                        {
                            k = b[index].ToString() + b[index + 1].ToString();
                            index++;
                        }
                        else
                        {
                            k = b[index].ToString();
                        }
                        if (characterFirst == false)
                        {

                            output.Add(k);
                            w = k;
                            characterFirst = true;
                            continue; //tiếp tục vòng lặp
                        }
                        //là kí tự bình thường, không có dạng là code
                        if (k != "[")
                        {
                            entry = k;
                            output.Add(entry);
                            if (entry == "\r\n")
                            {
                                dictionaryCharacter.Add("[" + numberOfCode.ToString() + "]", w + entry.Substring(0, 2));
                            }
                            else
                            {
                                dictionaryCharacter.Add("[" + numberOfCode.ToString() + "]", w + entry.Substring(0, 1));
                            }
                            numberOfCode++;
                            w = entry;
                            continue;
                        }
                        //check co phai code khong
                        if (k == "[")
                        {
                            String cBuffer = k;
                            for (int a = index + 1; a < b.Length; a++)
                            {
                                cBuffer = cBuffer + b[a];
                                if (b[a].ToString() == "]")  //trường hợp có dạng là code
                                {
                                    if (dictionaryCharacter.ContainsKey(cBuffer))
                                    { //nếu code có tồn tại trong dic
                                        entry = dictionaryCharacter.GetValueOrDefault(cBuffer);
                                        output.Add(entry);
                                        if (entry == "\r\n")
                                        {
                                            dictionaryCharacter.Add("[" + numberOfCode.ToString() + "]", w + entry.Substring(0, 2));
                                        }
                                        else
                                        {
                                            dictionaryCharacter.Add("[" + numberOfCode.ToString() + "]", w + entry.Substring(0, 1));
                                        }
                                        numberOfCode++;
                                        w = entry;
                                        index = a; //tăng index lên bằng với a
                                        break;
                                    }
                                    else //nếu code không tồn tại trong dic
                                    {
                                        if (w == "\r\n")
                                        {
                                            entry = w + w.Substring(0, 2);
                                        }
                                        else
                                        {
                                            entry = w + w.Substring(0, 1);
                                        }
                                        output.Add(entry);
                                        dictionaryCharacter.Add("[" + numberOfCode.ToString() + "]", entry);
                                        numberOfCode++;
                                        w = entry;
                                        index = a; //tăng index lên bằng với a
                                        break;
                                    }
                                }
                                else
                                {   //neu khong thi tiếp tục vòng lặp để xác định tiếp code
                                    continue;
                                }
                            }

                        }
                    }
                }
                String outputString = null;
                for (int a = 0; a < output.Count(); a++)
                {
                    outputString = outputString + output[a];
                }
                List<String> returnValue = new List<String>();
                returnValue.Add(outputString);
                returnValue.Add(inputString);

                return returnValue;
            }
            else
            {
                return null;
            }
        }
        public List<string> EncodeLZW(IFormFile Fileinput)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(Fileinput.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }
            String inputString = result.ToString();
            List<string> output = new List<string>();
            String w = "";
            char[] b = new char[inputString.Length];
            String k = "";
            using (StringReader sr = new StringReader(inputString))
            {
                Encoding ascii = Encoding.ASCII;
                sr.Read(b, 0, inputString.Length);
                Dictionary<String, String> dictionary = new Dictionary<String, String>();
                bool bufferFirst = false;
                int numberOfCode = 256;
                for (int i = 0; i < b.Length; i++)
                {

                    if (b[i] == '\r' && b[i + 1] == '\n')
                    {
                        k = b[i].ToString() + b[i + 1].ToString();
                        i++;
                    }
                    else
                    {
                        k = b[i].ToString();
                    }
                    if (bufferFirst == false)
                    {
                        //  output.Add(k);
                        bufferFirst = true;
                        w = k;
                        continue;
                    }

                    String wk = w + k;
                    if (dictionary.ContainsKey(wk))
                    {
                        w = wk;
                        if (i == b.Length - 1)
                        {
                            output.Add(w);
                        }
                    }
                    else
                    {
                        dictionary.Add(wk, "[" + numberOfCode.ToString() + "]");
                        numberOfCode++;
                        if (w.Length == 1 || w == "\r\n")
                        {
                            output.Add(w);
                        }
                        else
                        {
                            output.Add(dictionary.GetValueOrDefault(w));
                        }

                        w = k;
                        if (i == b.Length - 1)
                        {
                            output.Add(w);
                        }
                        continue;

                    }
                }

                String outputString = null;
                for (int a = 0; a < output.Count(); a++)
                {
                    outputString = outputString + output[a];
                }
                List<String> returnValue = new List<String>();
                returnValue.Add(outputString);
                returnValue.Add(inputString);

                return returnValue;
            }
        }
    }
}
