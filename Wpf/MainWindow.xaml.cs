// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// contributed by Ben Zuill-Smith (https://github.com/bzuillsmith)
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.OidcClient;
using System;
using System.Windows;
using IdentityModel.OidcClient.Browser;
using WpfSample.Auth;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OidcClient _oidcClient = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var options = new OidcClientOptions()
            {
                Authority = "http://localhost:5001",
                ClientId = "8b25bce5-fbee-41a8-ba0e-8385fe4afff7",
                ClientSecret = "8b25bce5-fbee-41a8-ba0e-8385fe4afff7",
                Scope = "openid profile openIdConnectClient",
                RedirectUri = "http://127.0.0.1/sample-wpf-app",
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.FormPost,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                Browser = new WpfEmbeddedBrowser()
            };

            _oidcClient = new OidcClient(options);

            LoginResult result;
            try
            {
                result = await _oidcClient.LoginAsync(new LoginRequest { BrowserDisplayMode = DisplayMode.Visible });
            }
            catch (Exception ex)
            {
                Message.Text = $"Unexpected Error: {ex.Message}";
                return;
            }

            if (result.IsError)
            {
                Message.Text = result.Error == "UserCancel" ? "A janela de autenticação foi fechada antes da autorização ser completada." : result.Error;
            }
            else
            {
                var name = result.User.Identity.Name;
                Message.Text = $"Hello {name}";
            }
        }
    }
}
