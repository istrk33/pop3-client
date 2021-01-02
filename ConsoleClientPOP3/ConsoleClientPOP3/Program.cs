using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClientPOP3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Démarrage du client POP3 - Version 2020 ETUDIANT");

            TcpClient socketClient = connexion(Preferences.serverName, Preferences.port);
            travail(socketClient);
            socketClient.Close();

            Console.WriteLine("Fin du client -> Taper une touche pour terminer");
            Console.ReadLine();
        }

        static TcpClient connexion(string nomServeur, int port)
        {
            TcpClient socketClient = new TcpClient();   // équivaut à la primitive Socket (avec mode TCP)

            // Récupération de l'adresse IP à partir de nomServeur
            IPAddress adresse = IPAddress.Parse("127.0.0.1");
            bool trouve = false;
            IPAddress[] adresses = Dns.GetHostAddresses(nomServeur);
            foreach (IPAddress ip in adresses)
            {//on cherche la première adresse IPV4
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    trouve = true;
                    adresse = ip;
                    break;
                }
            }
            if (!trouve)
            {
                Console.WriteLine("Echec recherche IP serveur");
                Console.ReadLine();
                Environment.Exit(1);
            }

            // Connexion
            socketClient.Connect(adresse, port);
            return socketClient;
        }

        static void travail(TcpClient socketClient)
        {
            if (socketClient.Connected)
            {
                // Connexion ok, mise en place des Streams pour lecture et écriture par ligne
                StreamReader sr = new StreamReader(socketClient.GetStream(), Encoding.Default);
                StreamWriter sw = new StreamWriter(socketClient.GetStream(), Encoding.Default);
                sw.AutoFlush = true;

                Communication.echanges(sr, sw);
            }
        }
    }
}
