using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terminal.Gui;

namespace ProjektCRUD
{
    public class Produkt
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public decimal Cena { get; set; }
        public string Kategoria { get; set; }
        public DateTime DataUtworzenia { get; set; }
    }

    class Program
    {
        static List<Produkt> produkty = new List<Produkt>();
        static int nextId = 1;
        static ListView listaProduktow;
        static TextView logText;

        static void Main(string[] args)
        {
            Application.Init();

            // Menu
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem("_Plik", new MenuItem[] {
                    new MenuItem("_Eksport do CSV", "", EksportDoCSV),
                    new MenuItem("_Wyjście", "", () => Application.RequestStop())
                }),
                new MenuBarItem("_Produkty", new MenuItem[] {
                    new MenuItem("_Dodaj produkt", "", DodajProdukt),
                    new MenuItem("_Edytuj produkt", "", EdytujProdukt),
                    new MenuItem("_Usuń produkt", "", UsunProdukt),
                    new MenuItem("_Pokaż wszystkie", "", PokazProdukty)
                })
            });

            // Główne okno
            var mainWindow = new Window("System Zarządzania Produktami")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Panel z przyciskami
            var buttonPanel = new FrameView("Akcje")
            {
                X = 0,
                Y = 0,
                Width = 25,
                Height = 10
            };

            var btnDodaj = new Button("_Dodaj produkt")
            {
                X = 1,
                Y = 0,
                Width = 20
            };
            btnDodaj.Clicked += DodajProdukt;

            var btnEdytuj = new Button("_Edytuj produkt")
            {
                X = 1,
                Y = 2,
                Width = 20
            };
            btnEdytuj.Clicked += EdytujProdukt;

            var btnUsun = new Button("_Usuń produkt")
            {
                X = 1,
                Y = 4,
                Width = 20
            };
            btnUsun.Clicked += UsunProdukt;

            var btnPokaz = new Button("_Pokaż wszystkie")
            {
                X = 1,
                Y = 6,
                Width = 20
            };
            btnPokaz.Clicked += PokazProdukty;

            buttonPanel.Add(btnDodaj, btnEdytuj, btnUsun, btnPokaz);

            // Lista produktów
            var listaFrame = new FrameView("Lista produktów")
            {
                X = 25,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 8
            };

            listaProduktow = new ListView(new List<string>())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            listaFrame.Add(listaProduktow);

            // Panel logów
            var logFrame = new FrameView("Logi systemowe")
            {
                X = 0,
                Y = Pos.Bottom(buttonPanel),
                Width = 25,
                Height = Dim.Fill() - 8
            };

            logText = new TextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true
            };

            logFrame.Add(logText);

            // Status bar
            var statusBar = new StatusBar()
            {
                Items = new StatusItem[] {
                    new StatusItem(Key.F1, "~F1~ Pomoc", () => PokazPomoc()),
                    new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Wyjście", () => Application.RequestStop()),
                    new StatusItem(Key.Null, $"Produkty: {produkty.Count}", null)
                }
            };

            mainWindow.Add(buttonPanel, listaFrame, logFrame);

            var top = Application.Top;
            top.Add(menu, mainWindow, statusBar);

            DodajLog("System uruchomiony. Gotowy do pracy.");
            AktualizujListe();

            Application.Run(top);
            Application.Shutdown();
        }

        static void AktualizujListe()
        {
            var items = produkty.Select(p => $"{p.Id}: {p.Nazwa} - {p.Cena:C}").ToList();
            listaProduktow.SetSource(items);
            DodajLog($"Zaktualizowano listę. Produktów: {produkty.Count}");
        }

        static void DodajLog(string wiadomosc)
        {
            logText.Text += $"{DateTime.Now:HH:mm:ss}: {wiadomosc}\n";
            // Przewiń do końca
            logText.MoveEnd();
        }

        static void PokazPomoc()
        {
            MessageBox.Query("Pomoc",
                "SYSTEM ZARZĄDZANIA PRODUKTAMI\n\n" +
                "Funkcje:\n" +
                "• Dodaj produkt - tworzy nowy produkt\n" +
                "• Edytuj produkt - modyfikuje istniejący\n" +
                "• Usuń produkt - usuwa wybrany produkt\n" +
                "• Pokaż wszystkie - wyświetla listę\n" +
                "• Eksport CSV - zapis do pliku\n\n" +
                "Skróty: F1-Pomoc, Ctrl+Q-Wyjście", "OK");
        }

        static void DodajProdukt()
        {
            var dialog = new Dialog("Dodawanie nowego produktu", 50, 14);

            var lblNazwa = new Label("Nazwa produktu:") { X = 1, Y = 1 };
            var txtNazwa = new TextField("") { X = 1, Y = 2, Width = Dim.Fill() - 2 };

            var lblCena = new Label("Cena:") { X = 1, Y = 4 };
            var txtCena = new TextField("") { X = 1, Y = 5, Width = Dim.Fill() - 2 };

            var lblKategoria = new Label("Kategoria:") { X = 1, Y = 7 };
            var txtKategoria = new TextField("") { X = 1, Y = 8, Width = Dim.Fill() - 2 };

            var btnOK = new Button("_Zapisz") { X = 1, Y = 11 };
            var btnAnuluj = new Button("_Anuluj") { X = 12, Y = 11 };

            btnOK.Clicked += () =>
            {
                if (string.IsNullOrWhiteSpace(txtNazwa.Text.ToString()))
                {
                    MessageBox.ErrorQuery("Błąd", "Nazwa produktu jest wymagana!", "OK");
                    return;
                }

                if (!decimal.TryParse(txtCena.Text.ToString(), out decimal cena) || cena < 0)
                {
                    MessageBox.ErrorQuery("Błąd", "Podaj prawidłową cenę!", "OK");
                    return;
                }

                var produkt = new Produkt
                {
                    Id = nextId++,
                    Nazwa = txtNazwa.Text.ToString().Trim(),
                    Cena = cena,
                    Kategoria = txtKategoria.Text.ToString().Trim(),
                    DataUtworzenia = DateTime.Now
                };

                produkty.Add(produkt);
                AktualizujListe();
                DodajLog($"DODANO: {produkt.Nazwa} za {produkt.Cena:C}");
                Application.RequestStop();
            };

            btnAnuluj.Clicked += () => Application.RequestStop();

            dialog.Add(lblNazwa, txtNazwa, lblCena, txtCena, lblKategoria, txtKategoria, btnOK, btnAnuluj);
            Application.Run(dialog);
        }

        static void EdytujProdukt()
        {
            if (listaProduktow.SelectedItem == -1 || listaProduktow.SelectedItem >= produkty.Count)
            {
                MessageBox.ErrorQuery("Błąd", "Wybierz produkt z listy do edycji!", "OK");
                return;
            }

            var produkt = produkty[listaProduktow.SelectedItem];
            var dialog = new Dialog($"Edycja: {produkt.Nazwa}", 50, 14);

            var lblNazwa = new Label("Nazwa:") { X = 1, Y = 1 };
            var txtNazwa = new TextField(produkt.Nazwa) { X = 1, Y = 2, Width = Dim.Fill() - 2 };

            var lblCena = new Label("Cena:") { X = 1, Y = 4 };
            var txtCena = new TextField(produkt.Cena.ToString()) { X = 1, Y = 5, Width = Dim.Fill() - 2 };

            var lblKategoria = new Label("Kategoria:") { X = 1, Y = 7 };
            var txtKategoria = new TextField(produkt.Kategoria) { X = 1, Y = 8, Width = Dim.Fill() - 2 };

            var btnZapisz = new Button("_Zapisz zmiany") { X = 1, Y = 11 };
            var btnAnuluj = new Button("_Anuluj") { X = 18, Y = 11 };

            btnZapisz.Clicked += () =>
            {
                if (string.IsNullOrWhiteSpace(txtNazwa.Text.ToString()))
                {
                    MessageBox.ErrorQuery("Błąd", "Nazwa produktu jest wymagana!", "OK");
                    return;
                }

                if (!decimal.TryParse(txtCena.Text.ToString(), out decimal cena) || cena < 0)
                {
                    MessageBox.ErrorQuery("Błąd", "Podaj prawidłową cenę!", "OK");
                    return;
                }

                string staraNazwa = produkt.Nazwa;
                produkt.Nazwa = txtNazwa.Text.ToString().Trim();
                produkt.Cena = cena;
                produkt.Kategoria = txtKategoria.Text.ToString().Trim();

                AktualizujListe();
                DodajLog($"EDYTOWANO: {staraNazwa} → {produkt.Nazwa}");
                Application.RequestStop();
            };

            btnAnuluj.Clicked += () => Application.RequestStop();

            dialog.Add(lblNazwa, txtNazwa, lblCena, txtCena, lblKategoria, txtKategoria, btnZapisz, btnAnuluj);
            Application.Run(dialog);
        }

        static void UsunProdukt()
        {
            if (listaProduktow.SelectedItem == -1 || listaProduktow.SelectedItem >= produkty.Count)
            {
                MessageBox.ErrorQuery("Błąd", "Wybierz produkt z listy do usunięcia!", "OK");
                return;
            }

            var produkt = produkty[listaProduktow.SelectedItem];

            if (MessageBox.Query("Potwierdzenie usunięcia",
                $"Czy na pewno chcesz usunąć produkt:\n\n\"{produkt.Nazwa}\"\nCena: {produkt.Cena:C}\nKategoria: {produkt.Kategoria}?",
                "Tak, usuń", "Nie") == 0)
            {
                produkty.RemoveAt(listaProduktow.SelectedItem);
                AktualizujListe();
                DodajLog($"USUNIĘTO: {produkt.Nazwa}");
                MessageBox.Query("Sukces", "Produkt został usunięty.", "OK");
            }
        }

        static void PokazProdukty()
        {
            if (produkty.Count == 0)
            {
                MessageBox.Query("Informacja", "Brak produktów w systemie.", "OK");
                return;
            }

            var dialog = new Dialog("Wszystkie produkty", 70, 20);
            var lista = new ListView(produkty.Select(p =>
                $"{p.Id,3}: {p.Nazwa,-20} {p.Cena,8:C} {p.Kategoria,-15} {p.DataUtworzenia:yyyy-MM-dd}").ToList())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1
            };

            var btnOK = new Button("_Zamknij") { X = Pos.Center(), Y = Pos.Bottom(lista) };
            btnOK.Clicked += () => Application.RequestStop();

            dialog.Add(lista, btnOK);
            Application.Run(dialog);
        }

        static void EksportDoCSV()
        {
            if (produkty.Count == 0)
            {
                MessageBox.ErrorQuery("Błąd", "Brak produktów do eksportu!", "OK");
                return;
            }

            try
            {
                using (var writer = new StreamWriter("produkty.csv"))
                {
                    writer.WriteLine("Id;Nazwa;Cena;Kategoria;DataUtworzenia");
                    foreach (var produkt in produkty)
                    {
                        writer.WriteLine($"{produkt.Id};{produkt.Nazwa};{produkt.Cena};{produkt.Kategoria};{produkt.DataUtworzenia}");
                    }
                }
                DodajLog($"EKSPORT: Utworzono plik produkty.csv z {produkty.Count} produktami");
                MessageBox.Query("Sukces", $"Pomyślnie wyeksportowano {produkty.Count} produktów do pliku produkty.csv", "OK");
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Błąd eksportu", $"Nie udało się zapisać pliku:\n{ex.Message}", "OK");
            }
        }
    }
}