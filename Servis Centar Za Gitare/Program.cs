using System;
using System.Collections.Generic;
using System.Linq;
using Servis_Centar_Za_Gitare.enums;
using Servis_Centar_Za_Gitare.models;

var tehnicar = new ZapTehnicar(new List<Znanje>
{
    new Znanje(TipGitareEnum.Elektricna, VrstaPopravkaEnum.ZamjenaZica),
    new Znanje(TipGitareEnum.Aukusticna, VrstaPopravkaEnum.PodesavanjeVrata)
})
{
    Id = 1,
    Ime = "Ivan",
    Prezime = "Ivic",
    Email = "ivan.ivic@servis.hr",
    BrojTelefona = "0911111111",
    Adresa = "Ilica 10, Zagreb",
    DatumZaposlenja = "2024-01-15"
};

var menadzer = new Zaposlenik(
    2,
    "Ana",
    "Anic",
    "ana.anic@servis.hr",
    "0922222222",
    "Savska 20, Zagreb",
    "2023-09-01",
    1500);

var gitara1 = new Gitara(1, "SN001", MarkeEnum.Fender, "6", TipGitareEnum.Elektricna, DateTime.Now.AddDays(-10), 1);
var gitara2 = new Gitara(2, "SN002", MarkeEnum.Yamaha, "6", TipGitareEnum.Aukusticna, DateTime.Now.AddDays(-5), 2);

var stranka1 = new Stranka(
    1,
    "Marko",
    "Markovic",
    "marko@email.com",
    "0953333333",
    "Dubrava 1, Zagreb",
    "2026-03-01",
    "Redovan kupac",
    new List<Gitara> { gitara1 });

var stranka2 = new Stranka(
    2,
    "Petra",
    "Petric",
    "petra@email.com",
    "0964444444",
    "Maksimir 2, Zagreb",
    "2026-03-05",
    "Zeli brzi servis",
    new List<Gitara> { gitara2 });

var nalog1 = new Nalog(
    gitara1,
    stranka1,
    tehnicar,
    "Pukla zica",
    DateTime.Now.AddDays(-2),
    DateTime.Now.AddDays(2),
    StatusNalogaEnum.Zaprimljen,
    VrstaPopravkaEnum.ZamjenaZica);

var nalog2 = new Nalog(
    gitara2,
    stranka2,
    tehnicar,
    "Visok action",
    DateTime.Now.AddDays(-1),
    DateTime.Now.AddDays(3),
    StatusNalogaEnum.UObradi,
    VrstaPopravkaEnum.PodesavanjeVrata);

var poslovnica = new Poslovnica(
    new List<ZapTehnicar> { tehnicar },
    new List<Zaposlenik> { menadzer },
    new List<Nalog> { nalog1, nalog2 },
    new List<Stranka> { stranka1, stranka2 },
    "Servis Centar Zagreb",
    "Radnicka cesta 50, Zagreb");

var otvoreniNalozi = poslovnica.Nalozi
    .Where(n => n.Status == StatusNalogaEnum.Zaprimljen || n.Status == StatusNalogaEnum.UObradi)
    .ToList();

var elektricneGitare = poslovnica.Gitare
    .Where(g => g.TipGitare == TipGitareEnum.Elektricna)
    .ToList();

var strankeSaGitarama = poslovnica.Stranke
    .Where(s => s.Gitare.Any())
    .ToList();

Console.WriteLine($"Poslovnica: {poslovnica.Ime}");
Console.WriteLine($"Adresa: {poslovnica.Adresa}");
Console.WriteLine($"Broj gitara: {poslovnica.Gitare.Count}");
Console.WriteLine($"Broj stranaka: {poslovnica.Stranke.Count}");
Console.WriteLine($"Broj zaposlenika: {poslovnica.Tehnicari.Count + poslovnica.Menadzeri.Count}");
Console.WriteLine($"Broj naloga: {poslovnica.Nalozi.Count}");

Console.WriteLine("\nOtvoreni nalozi:");
foreach (var nalog in otvoreniNalozi)
{
    Console.WriteLine($"- {nalog.Stranka.Ime} {nalog.Stranka.Prezime}: {nalog.OpisKvara} ({nalog.Status})");
}

Console.WriteLine("\nElektricne gitare:");
foreach (var gitara in elektricneGitare)
{
    Console.WriteLine($"- {gitara.Marka} ({gitara.SerijskiBroj})");
}

Console.WriteLine("\nStranke sa gitarama:");
foreach (var stranka in strankeSaGitarama)
{
    Console.WriteLine($"- {stranka.Ime} {stranka.Prezime} ({stranka.Gitare.Count} gitara)");
}
