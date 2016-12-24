using IdentityModel.OidcClient;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSystemBrowser
{
    class Program
    {
        static string _authority = "https://demo.identityserver.io";
        static string _api = "https://api.identityserver.io/identity";

        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            // setup logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            Console.WriteLine("+-----------------------+");
            Console.WriteLine("|  Sign in with OIDC    |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("");
            Console.WriteLine("Press any key to sign in...");
            Console.ReadKey();

            await SignInAsync();

            Console.ReadKey();
        }

        private static async Task SignInAsync()
        {
            // create a redirect URI using an available port on the loopback address.
            string redirectUri = string.Format("http://127.0.0.1:7890/");
            Console.WriteLine("redirect URI: " + redirectUri);

            // create an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Console.WriteLine("Listening..");
            http.Start();

            var options = new OidcClientOptions(
                _authority,
                "native.hybrid",
                "",
                "openid profile api",
                redirectUri)
            {
                UseFormPost = true,
                Style = OidcClientOptions.AuthenticationStyle.Hybrid
            };

            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            Console.WriteLine($"Start URL: {state.StartUrl}");

            // open system browser to start authentication
            Process.Start(state.StartUrl);

            // wait for the authorization response.
            var context = await http.GetContextAsync();

            var formData = GetRequestPostData(context.Request);

            // Brings the Console to Focus.
            BringConsoleToFront();

            // sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://demo.identityserver.io'></head><body>Please return to the app.</body></html>");
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();
            http.Stop();

            Console.WriteLine($"Form Data: {formData}");
            var result = await client.ValidateResponseAsync(formData, state);

            if (!result.Success)
            {
                Console.WriteLine("\n\nError:\n{0}", result.Error);
                return;
            }

            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            Console.WriteLine();
            Console.WriteLine("Access token:\n{0}", result.AccessToken);

            Console.WriteLine("\n\npress return to call an API");
            Console.ReadLine();

            var apiClient = new HttpClient();
            apiClient.SetBearerToken(result.AccessToken);

            var apiResponse = await apiClient.GetStringAsync(_api);
            Console.WriteLine(JArray.Parse(apiResponse));

            if (!string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                Console.WriteLine("Refresh token:\n{0}", result.RefreshToken);
                var refreshToken = result.RefreshToken;

                while (true)
                {
                    Console.WriteLine("\n\npress return to refresh token");
                    Console.ReadLine();

                    var refreshResult = await client.RefreshTokenAsync(refreshToken);
                    if (!refreshResult.Success)
                    {
                        Console.WriteLine(refreshResult.Error);
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Access token:\n{0}", refreshResult.AccessToken);
                        Console.WriteLine("Refresh token:\n{0}", refreshResult.RefreshToken);

                        refreshToken = refreshResult.RefreshToken;
                    }
                }
            }
        }

        // Hack to bring the Console window to front.
        // ref: http://stackoverflow.com/a/12066376
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringConsoleToFront()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream)
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}