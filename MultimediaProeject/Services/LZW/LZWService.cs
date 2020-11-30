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
        public List<string> EncodeLZW(IFormFile Fileinput)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(Fileinput.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(reader.ReadLine());
            }
            String inputString = result.ToString();   //đọc file ra string
            List<string> output = new List<string>(); //đầu ra 
            String w = "";       //bộ đệm chứa
            String k = "";       //đầu vào
            Dictionary<String, String> dictionary = new Dictionary<String, String>();// từ điển
            bool bufferFirst = true; //nhận biết đã đọc lí tự đầu tiên chưa
            char[] b = new char[inputString.Length]; //mảng lưu string
            int code = 256; //code
            using (StringReader sr = new StringReader(inputString))
            {
                sr.Read(b, 0, inputString.Length);   //chuyển stringinput sang mảng b để xử lý

                for (int i = 0; i < b.Length; i++)                  //dòng 3
                {

                    if (b[i] == '\r' && b[i + 1] == '\n')        //check có phải là 2 kí tự \r và \n không
                    {
                        k = b[i].ToString() + b[i + 1].ToString();
                        i++;
                    }
                    else
                    {
                        k = b[i].ToString();    //đọc từ mảng b sang string k
                    }
                    if (bufferFirst == true)  //nhận biết đây có phải kí tự đầu tiên không
                    {
                        //  output.Add(k);
                        bufferFirst = false;
                        w = k;
                        continue;
                    }

                    String wk = w + k;  //dòng 4
                    if (dictionary.ContainsKey(wk))  //dòng 5
                    {
                        w = wk;   //dòng 6
                        if (i == b.Length - 1) //check có phải kí tự cuối không
                        {
                            output.Add(w);
                        }
                    }
                    else  //(dòng 7)
                    {
                        dictionary.Add(wk, "[" + code.ToString() + "]"); //(dòng 8)  //add wk vào từ điển
                        code++;             //tăng code lên để dùng cho trường hợp tiếp
                        if (w.Length == 1 || w == "\r\n")    // (dòng 9)
                        {
                            output.Add(w); //dòng 10
                        }
                        else //dòng 11
                        {
                            output.Add(dictionary.GetValueOrDefault(w)); //dòng 12
                        }

                        w = k;  //dòng 13
                        if (i == b.Length - 1)//check có phải kí tự cuối không
                        {
                            output.Add(w);
                        }
                        continue;//tiếp tục chạy vòng lặp

                    }
                }
                //đẩy kết quả ra controller
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
                int code = 256;
                Dictionary<String, String> dictionaryCharacter = new Dictionary<string, string>();
                /*----------------------------------------------------*/
                using (StringReader sr = new StringReader(inputString))
                {
                    Encoding ascii = Encoding.ASCII;
                    sr.Read(b, 0, inputString.Length);  // Add các ký tự vào mảng ký tự b
                    /*------------------------------------------------*/
                    for (int index = 0; index < b.Length; index++)
                    {

                        if (b[index] == '\r' && b[index + 1] == '\n')   //check có phải kí tự \r và \n không
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
                        if (k != "[") //dòng 4: là một kí tự (không phải code)
                        {
                            entry = k;     //dòng 5
                            output.Add(entry);      //dòng 6
                            if (entry == "\r\n")  //check entry có phải cụm kí tự \r\n không
                            {
                                dictionaryCharacter.Add("[" + code.ToString() + "]", w + entry.Substring(0, 2)); //dòng 7 trường hợp là cụm kí tự \r\n
                            }
                            else
                            {
                                dictionaryCharacter.Add("[" + code.ToString() + "]", w + entry.Substring(0, 1)); //dòng 7 trường hợp không là cụm kí tự \r\n
                            }
                            code++;    //tăng code lên dùng cho lần sau
                            w = entry;        //dòng 8
                            continue; //tiếp tục vòng lặp
                        }
                        if (k == "[") //dòng 4: trường hợp là một code
                        {
                            String cBuffer = k;//biến tạm lưu giá trị của code
                            for (int a = index + 1; a < b.Length; a++)   //mục đích của vòng lặp này là sẽ chạy các kí tự tiếp từ kí tự "[" đến khi nào tìm đến kí tự "]" => có dạng là một code
                            {
                                cBuffer = cBuffer + b[a];
                                if (b[a].ToString() == "]")  //trường hợp có dạng là code
                                {
                                    if (dictionaryCharacter.ContainsKey(cBuffer)) //dòng 4
                                    { //nếu code có tồn tại trong dic
                                        entry = dictionaryCharacter.GetValueOrDefault(cBuffer);//dòng 5
                                        output.Add(entry);   //dòng 6
                                        if (entry == "\r\n")
                                        {
                                            dictionaryCharacter.Add("[" + code.ToString() + "]", w + entry.Substring(0, 2));      //dòng 7
                                        }
                                        else
                                        {
                                            dictionaryCharacter.Add("[" + code.ToString() + "]", w + entry.Substring(0, 1));    //dòng 7
                                        }
                                        code++;
                                        w = entry;   //dòng 8
                                        index = a; //tăng index lên bằng với a
                                        break;
                                    }
                                    else //nếu code không tồn tại trong dic
                                    {
                                        if (w == "\r\n")            //dòng 10
                                        {
                                            entry = w + w.Substring(0, 2);
                                        }
                                        else
                                        {
                                            entry = w + w.Substring(0, 1);
                                        }
                                        output.Add(entry); //dòng 10.1
                                        dictionaryCharacter.Add("[" + code.ToString() + "]", entry);   //dòng11
                                        code++;
                                        w = entry;    //dòng 12
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
    }
}
