using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Citacoes.Resources;
using Citacoes.Classes;
using System.IO.IsolatedStorage;
using Newtonsoft.Json.Linq;

namespace Citacoes
{
    public partial class MainPage : PhoneApplicationPage
    {
        public clsCitacoes citacao;
        public List<clsCitacoes> ltCitacoes = new List<clsCitacoes>();
        private IsolatedStorageSettings appPreferencias = IsolatedStorageSettings.ApplicationSettings;
        private bool statusRede { get; set; }

        public MainPage()
        {
            InitializeComponent();

            progbarLoad.Visibility = System.Windows.Visibility.Collapsed;
            txtBusca.Visibility = Visibility.Visible;
            txbBusca.Visibility = Visibility.Visible;
            lstCitacoes.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (appPreferencias.Contains("nomeUsuario"))
            {
                txbNome.Text = "olá, " + appPreferencias["nomeUsuario"].ToString() + "!";
                lstCitacoes.ItemsSource = clsCitacoesDB.listar();
            }
            else
            {
                txbNome.Text = "olá!";

                clsCitacoesDB.limpar();
                carregarCitacoes();
                
                MessageBox.Show("Olá! \n\nComo é seu primeiro acesso, você precisa informar o seu nome.");
                NavigationService.Navigate(new Uri("/PrimeiroAcesso.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            MostraCitacao page = e.Content as MostraCitacao;
            if (page != null)
            {
                page.citacao = citacao;
            }
        }

        public void carregarCitacoes()
        {
            progbarLoad.Visibility = System.Windows.Visibility.Visible;
            txtBusca.Visibility = Visibility.Collapsed;
            txbBusca.Visibility = Visibility.Collapsed;
            lstCitacoes.Visibility = Visibility.Collapsed;

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
            webClient.DownloadStringAsync(new Uri(@"page"), UriKind.Absolute);
        }

        public void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                String JSON = e.Result;
                loadJSON(JSON);
                statusRede = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (statusRede)
                {
                    lstCitacoes.ItemsSource = clsCitacoesDB.listar();

                    progbarLoad.Visibility = System.Windows.Visibility.Collapsed;
                    txtBusca.Visibility = Visibility.Visible;
                    txbBusca.Visibility = Visibility.Visible;
                    lstCitacoes.Visibility = Visibility.Visible;
                }
                else
                {
                    progbarLoad.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public void loadJSON(string pJSON)
        {
            if (pJSON != null)
            {
                JObject jobject = JObject.Parse(pJSON);
                JArray citacoes = (JArray)jobject["citacoes"];

                foreach (JObject citacao in citacoes)
                {
                    clsCitacoes c = new clsCitacoes()
                    {
                        id = (Int32)citacao["id"],
                        citacao = (String)citacao["citacao"],
                        autor = (String)citacao["autor"],
                        categoria = (String)citacao["categoria"],
                        slug = (String)citacao["slug"]
                    };

                    if (c != null)
                    {
                        clsCitacoesDB.salvar(c);
                    }
                }
            }
        }

        private void txbBusca_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<clsCitacoes> lista = clsCitacoesDB.SelecionaAutor(txbBusca.Text);
            lstCitacoes.ItemsSource = lista;
        }

        private void OnClickPreferencias(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/PrimeiroAcesso.xaml", UriKind.Relative));
        }

        private void OnClickAtualizar(object sender, EventArgs e)
        {
            clsCitacoesDB.limpar();
            carregarCitacoes();
        }

        private void OnClickCategoria(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MostraCategorias.xaml", UriKind.Relative));
        }

        private void lstCitacoes_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            citacao = (sender as ListBox).SelectedItem as clsCitacoes;
            if (citacao != null)
            {
                NavigationService.Navigate(new Uri("/MostraCitacao.xaml", UriKind.Relative));
            }
        }

        private void OnClickFavoritos(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MostraFavoritos.xaml", UriKind.Relative));
        }
    }
}