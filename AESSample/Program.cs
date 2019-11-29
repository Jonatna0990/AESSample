using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace AESSample
{
    class Program
    {
        static void Main(string[] args)
        {
            UserActivate();
            Console.ReadKey();
        }

        private static int count = 1;

        private static int error_count = 0;

        static string host = "https://belogex.com/api/payment/invoke.php";
        //static string host = "http://bx.lite/invoke.php";


        static async void UserUpdate(string email, string serial)
        {
            Console.WriteLine("UserUpdate");
            try
            {
                HttpClient client = new HttpClient();

               
                var tok = SecurityHelper.CreateSHA256(email + serial + "hw98h38g9q87g4");
                var data = SecurityHelper.CreateMD5(tok + serial + email);
                var par = string.Format("{0}?method=user.update&email={1}&serial_number={2}&access_token={3}&data={4}", host, email, serial, tok, data);
                var responseString = await client.GetStringAsync(par);
                var g = responseString;
                Token token = JsonConvert.DeserializeObject<Token>(g);
                if (string.IsNullOrEmpty(token.access_token) && string.IsNullOrEmpty(token.data))
                {
                    Error error = JsonConvert.DeserializeObject<Error>(g);
                    if (error != null)
                    {
                        Console.WriteLine(error.message);
                        Thread.Sleep(1000);
                        UserUpdate(email, serial);
                    }
                }
                else
                {
                    var token_new = SecurityHelper.CreateMD5(token.access_token + "hw98h38g9q87g4");
                    string decrypted = SecurityHelper.Decrypt(new Tuple<string, string>(token.data, token_new));
                    Console.WriteLine(decrypted + "  token.access_token " + token.access_token + "   " + count);
                    Response response = JsonConvert.DeserializeObject<Response>(decrypted);
                    if (response.status == 207)
                    {
                        UserActivate(email, serial);
                    }

                }



                // Response message = JsonConvert.DeserializeObject<Response>(decrypted);
                // Console.WriteLine(message.status);
                // Console.WriteLine(message.random);
                // $access_token_new = md5($access_token. 'hw98h38g9q87g4');

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static async void UserActivate(string email = "gazaryan10@bk.ru", string serial = "27AF0-7AF02-4C1EB-2C0DF")
        {
            Console.WriteLine("UserActivate");
            try
            {
                HttpClient client = new HttpClient();
              
                var par = string.Format("{0}?method=user.activate&email={1}&serial_number={2}", host, email, serial);
                var responseString = await client.GetStringAsync(par);
                var g = responseString;
                Token token = JsonConvert.DeserializeObject<Token>(g);
                if (string.IsNullOrEmpty(token.access_token) && string.IsNullOrEmpty(token.data))
                {
                    Error error = JsonConvert.DeserializeObject<Error>(g);
                    if (error != null)
                    {
                        if (error_count < 3)
                        {
                            Console.WriteLine(error.message);
                            Thread.Sleep(1500);
                            UserActivate(email, serial);
                            error_count++;
                        }
                        else
                        {
                            UserUpdate(email, serial);
                            error_count = 0;
                            // UserActivate(email, serial);
                        }
                    }
                }
                else
                {
                    var token_new = SecurityHelper.CreateMD5(token.access_token + "hw98h38g9q87g4");
                    string decrypted = SecurityHelper.Decrypt(new Tuple<string, string>(token.data, token_new));
                    Console.WriteLine(decrypted + "  token.access_token " + token.access_token+"   " + count);
                    Response response = JsonConvert.DeserializeObject<Response>(decrypted);
                    if (response.status == 100)
                    {
                        UserLogin(email, serial, token.access_token);
                    }

                }



                // Response message = JsonConvert.DeserializeObject<Response>(decrypted);
                // Console.WriteLine(message.status);
                // Console.WriteLine(message.random);
                // $access_token_new = md5($access_token. 'hw98h38g9q87g4');

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static async void UserLogin(string email, string serial, string access_token)
        {
            Console.WriteLine("UserLogin");

            try
            {
                HttpClient client = new HttpClient();
                var user = new User() { email = email, random_body = SecurityHelper.RandomString(15), random_footer = SecurityHelper.RandomString(15), random_header = SecurityHelper.RandomString(15) , serial_number = serial };
                var hhh = JsonConvert.SerializeObject(user);

              
         
                string token_new = SecurityHelper.CreateMD5(access_token+"hw98h38g9q87g4");
                string encrypted = SecurityHelper.Encrypt(new Tuple<string, string>(hhh, token_new));

                var par = string.Format("{0}?method=user.login&access_token={1}&data={2}", host, access_token, encrypted);
                var responseString = await client.GetStringAsync(par); 
                var g = responseString;
                Token token = JsonConvert.DeserializeObject<Token>(g);
                if (string.IsNullOrEmpty(token.access_token) && string.IsNullOrEmpty(token.data))
                {
                    Error error = JsonConvert.DeserializeObject<Error>(g);
                    if (error != null)
                    {
                        Console.WriteLine(error.message);
                    }
                }
                else
                {
                    token_new = SecurityHelper.CreateMD5(token.access_token + "hw98h38g9q87g4");
                    string decrypted = SecurityHelper.Decrypt(new Tuple<string, string>(token.data, token_new));
                    Console.WriteLine(decrypted + "  "+ count);
                    Response response = JsonConvert.DeserializeObject<Response>(decrypted);
                    if (response.status == 100)
                    {
                        if (count < 5)
                        {
                            Thread.Sleep(1500);
                            count++;
                            UserLogin(email, serial, token.access_token);

                        }
                        else
                        {
                            count = 1;
                            UserActivate();
                        }


                    }

                }



                // Response message = JsonConvert.DeserializeObject<Response>(decrypted);
                // Console.WriteLine(message.status);
                // Console.WriteLine(message.random);
                // $access_token_new = md5($access_token. 'hw98h38g9q87g4');

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
