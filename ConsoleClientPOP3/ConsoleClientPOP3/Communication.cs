using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClientPOP3
{
    class Communication
    {
        static bool deleted = false;
        #region Debug avec VERBOSE
        private static bool VERBOSE = true;

        //private static bool VERBOSE = false;
        private static void etatVERBOSE()
        {
            Console.WriteLine(VERBOSE ? "VERBOSE on" : "VERBOSE off");
        }
        #endregion
        #region Méthodes de lecture et écriture
        private static void LireLigne(StreamReader input, out string ligne)
        {
            // Lecture d'une ligne dans le Stream associé à la socket (en provenance du serveur POP3)
            ligne = input.ReadLine();
            // Si le mode "Debug" est activé :
            // Affiche à l'écran ce qui vient d'être lu dans la socket, précédé du mot "reçu >> "
            if (VERBOSE)
                Console.WriteLine("     recu  >> " + ligne);
        }
        private static void EcrireLigne(StreamWriter output, string ligne)
        {
            // Ecriture d'une ligne dans le Stream associé à la socket (à destination du serveur POP3)
            output.WriteLine(ligne);
            // Si le mode "Debug" est activé :
            // Affiche à l'écran ce qui vient d'être écrit dans la socket, précédé du mot "envoi << "
            if (VERBOSE)
                Console.WriteLine("     envoi << " + ligne);
        }
        #endregion
        #region Méthode erreur fatale
        private static void FATAL(string Message)
        {
            Console.WriteLine(Message);
            Console.ReadLine();
            Environment.Exit(1);
        }
        #endregion

        public static void echanges(StreamReader sr, StreamWriter sw)
        {
            string ligne, tampon;

            /* reception banniere */
            LireLigne(sr, out ligne);  // ou  ligne = sr.ReadLine();
            if (ligne[0] != '+')
            {
                FATAL("Pas de banniere. Abandon");
            };

            /* envoi identification */
            tampon = "USER " + Preferences.username;
            EcrireLigne(sw, tampon);   // ou  sw.WriteLine(tampon);
            LireLigne(sr, out ligne);  // ou  ligne = sr.ReadLine();
            if (ligne[0] != '+')
            {
                FATAL("USER rejeté. Abandon");
            };

            /* envoi mot de passe */
            tampon = "PASS " + Preferences.password;
            EcrireLigne(sw, tampon);
            LireLigne(sr, out ligne);
            if (ligne[0] != '+')
            {
                FATAL("PASS rejeté. Abandon");
            }

            /* envoi STAT pour recuperer nb messages */
            stat(sw, sr);

            /* Menu des fonctionnalités et traitements */
            bool fin = false;
            while (!fin)
            {
                afficher_menu();
                fin = traiter_menu(sw, sr);
            }
        }

        /**
         * Afficher les options du menu 
         */
        static void afficher_menu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n");
            Console.WriteLine("------MENU-----------------------------------------------------------------");
            Console.WriteLine("s. Afficher le nb de messages (STAT)");
            Console.WriteLine("r. Afficher le n-ième message (RETR n)");
            Console.WriteLine("n. Afficher uniquement le nom de l'expéditeur et le sujet du n-ième message");
            Console.WriteLine("all. Afficher le nom de l’expéditeur et le sujet de tous les messages. ");
            Console.WriteLine("d. Supprime le message numéro n (DELE n)");
            Console.WriteLine("dall. Supprime tout les messages ");
            Console.WriteLine("res. Annule la suppression de tous les messages (RSET)");
            Console.WriteLine("nope. Ne fait rien (NOOP)");
            Console.WriteLine("v. Debug (VERBOSE on/off)");
            Console.WriteLine("q. Quitter");
            Console.WriteLine("---------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
        }

       /**
         * Gestion du menu 
         */
        static bool traiter_menu(StreamWriter sw, StreamReader sr)
        {
            bool fin = false;
            string choix = Console.ReadLine();
            switch (choix)
            {
                case "s":
                    stat(sw, sr);
                    break;
                case "v":
                    VERBOSE = !VERBOSE;
                    etatVERBOSE();
                    break;
                case "r":
                    Console.WriteLine("Le numero du mail ?");
                    int n = Int32.Parse(Console.ReadLine());
                    retr(sw, sr, n);
                    break;
                case "n":
                    Console.WriteLine("Le numero du mail ?");
                    int num = Int32.Parse(Console.ReadLine());
                    oneMail(sw, sr, num);
                    break;
                case "all":
                    allMails(sw, sr);
                    break;
                case "d":
                    Console.WriteLine("Le numero du mail ?");
                    int numb = Int32.Parse(Console.ReadLine());
                    deleteOneMail(sw, sr, numb);
                    deleted = true;
                    break;
                case "dall":
                    deleteAllMails(sw, sr);
                    deleted = true;
                    break;
                case "res":
                    if (deleted)
                    {
                        resetDeletion(sw, sr);
                        deleted = false;
                    }else{
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Aucune suppression d'actualité .");
                    Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
                case "q":
                    quitter(sw, sr);
                    fin = true;
                    break;
                case "nope":
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("On fait rien bro, ne me dérange pas pour rien XD");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    break;
            }
            return fin;
        }


        /* Afficher le nb de messages (STAT) */
        static void stat(StreamWriter sw, StreamReader sr)
        {
            string ligne, tampon;

            /* envoi STAT pour recuperer nb messages */
            tampon = "STAT";
            EcrireLigne(sw, tampon);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);
            string[] lesValeurs = ligne.Split(' ');
            int nombre_de_messages = Int32.Parse(lesValeurs[1]);
            int taille_boite = Int32.Parse(lesValeurs[2]);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Il y a " + nombre_de_messages.ToString() + " messages dans la boite.\n");
            Console.Write("La taille totale est de " + taille_boite.ToString() + " octets.\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /* Afficher le n-ième message (RETR) */
        static void retr(StreamWriter sw, StreamReader sr, int i)
        {
            string ligne, tampon;

            /* envoi RETR pour recuperer nb messages */
            tampon = "RETR";
            EcrireLigne(sw, tampon + " " + i);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);

            Console.ForegroundColor = ConsoleColor.Blue;

            bool contenu = false;
            while (sr.Peek() >= 0)
            {
                string ligneCourrante = sr.ReadLine();
                if (contenu && !ligneCourrante.Equals(".") && !ligneCourrante.StartsWith("X-AV-Checked"))
                {
                    Console.Write(ligneCourrante + "\n");
                }
                if (ligneCourrante.StartsWith("From:") || ligneCourrante.StartsWith("To:") || ligneCourrante.StartsWith("Subject:") || ligneCourrante.StartsWith("Date:"))
                {
                    Console.Write(ligneCourrante + "\n");
                }
                else if (ligneCourrante.StartsWith("Content-Transfer-Encoding:"))
                {
                    contenu = true;
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        /* Afficher expéditeur et sujet d'un message*/
        static void oneMail(StreamWriter sw, StreamReader sr, int i)
        {
            string ligne, tampon;
            /* envoi RETR pour recuperer nb messages */
            tampon = "RETR";
            EcrireLigne(sw, tampon + " " + i);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);

            Console.ForegroundColor = ConsoleColor.Blue;
            while (sr.Peek() >= 0)
            {
                string ligneCourrante = sr.ReadLine();
                if (ligneCourrante.StartsWith("From:"))
                {
                    Console.Write("Expéditeur du message : " + ligneCourrante.Split(':')[1] + "\n");
                }
                if (ligneCourrante.StartsWith("Subject:"))
                {
                    Console.Write("Sujet : " + ligneCourrante.Split(':')[1] + "\n");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        /* Afficher expéditeur et sujet de tout les messages*/
        static void allMails(StreamWriter sw, StreamReader sr)
        {
            string ligne, tampon;
            int nbMessages;

            /* envoi RETR pour recuperer nb messages */
            tampon = "STAT";
            EcrireLigne(sw, tampon);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);
            nbMessages = Int32.Parse(ligne.Split(' ')[1]);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Il y a " + nbMessages + " messages\n");
            for (int i = 1; i <= nbMessages; i++)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Message " + i + " :\n");
                oneMail(sw, sr, i);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        /* Supprimer un message (DELE)*/
        static void deleteOneMail(StreamWriter sw, StreamReader sr, int i)
        {
            string ligne, tampon;

            /* envoi RETR pour recuperer nb messages */
            tampon = "DELE ";
            EcrireLigne(sw, tampon + i);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Message " + i + " supprimé !\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /*Supprimer tout les messages*/
        static void deleteAllMails(StreamWriter sw, StreamReader sr)
        {
            string ligne, tampon;
            int nbMessages;

            /* envoi RETR pour recuperer nb messages */
            tampon = "STAT";
            EcrireLigne(sw, tampon);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);
            nbMessages = Int32.Parse(ligne.Split(' ')[1]);
            
            for (int i = 1; i <= nbMessages; i++)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                deleteOneMail(sw, sr, i);
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("\nTout les messages supprimés !!!\n\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /*Annuler toutes les suppression d'actualité*/
        static void resetDeletion(StreamWriter sw, StreamReader sr)
        {
            string ligne, tampon;

            /* envoi RETR pour recuperer nb messages */
            tampon = "RSET";
            EcrireLigne(sw, tampon);

            /* reception de +OK n mm */
            LireLigne(sr, out ligne);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Suppression annulé.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /* Arrêter la communication (QUIT) */
        static void quitter(StreamWriter sw, StreamReader sr)
        {
            string ligne, tampon;

            /* envoi QUIT pour arreter la communication */
            tampon = "QUIT";
            EcrireLigne(sw, tampon);
            LireLigne(sr, out ligne); // lecture du +OK
        }

    }
}
