using System;
using System.Collections.Generic;
using System.Linq;
using Servis_Centar_Za_Gitare.enums;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Mock
{
    public static class GuitarServiceMockData
    {
        private static readonly List<ZapTehnicar> _technicians = new();
        private static readonly List<Zaposlenik> _managers = new();
        private static readonly List<Stranka> _customers = new();
        private static readonly List<Gitara> _guitars = new();
        private static readonly List<Nalog> _repairs = new();
        private static readonly Poslovnica _office;

        static GuitarServiceMockData()
        {
            var technicianOne = new ZapTehnicar(
                1,
                "Ivan",
                "Ivic",
                "ivan.ivic@servis.hr",
                "0911111111",
                "Ilica 10, Zagreb",
                "2024-01-15",
                1800,
                new List<Znanje>
                {
                    new Znanje(TipGitareEnum.Elektricna, VrstaPopravkaEnum.ZamjenaZica),
                    new Znanje(TipGitareEnum.Aukusticna, VrstaPopravkaEnum.PodesavanjeVrata),
                    new Znanje(TipGitareEnum.Elektricna, VrstaPopravkaEnum.PopravakElektronike)
                });

            var technicianTwo = new ZapTehnicar(
                2,
                "Lea",
                "Lasic",
                "lea.lasic@servis.hr",
                "0922222222",
                "Savska 20, Zagreb",
                "2023-09-01",
                1850,
                new List<Znanje>
                {
                    new Znanje(TipGitareEnum.Klasicna, VrstaPopravkaEnum.PodesavanjeIntonacije),
                    new Znanje(TipGitareEnum.BasGitara, VrstaPopravkaEnum.ZamjenaMasinica)
                });

            _technicians.AddRange(new[] { technicianOne, technicianTwo });

            _managers.AddRange(new[]
            {
                new Zaposlenik(10, "Ana", "Anic", "ana.anic@servis.hr", "0933333333", "Radnicka 50, Zagreb", "2023-05-10", 2200),
                new Zaposlenik(11, "Marko", "Maric", "marko.maric@servis.hr", "0944444444", "Radnicka 50, Zagreb", "2022-11-01", 2400)
            });

            var customerOne = new Stranka(
                1,
                "Marko",
                "Markovic",
                "marko@email.com",
                "0953333333",
                "Dubrava 1, Zagreb",
                "2026-03-01",
                "Redovan kupac",
                new List<Gitara>());

            var customerTwo = new Stranka(
                2,
                "Petra",
                "Petric",
                "petra@email.com",
                "0964444444",
                "Maksimir 2, Zagreb",
                "2026-03-05",
                "Zeli brzi servis",
                new List<Gitara>());

            var customerThree = new Stranka(
                3,
                "Tomislav",
                "Kovac",
                "tomislav.kovac@email.com",
                "0975555555",
                "Trešnjevka 8, Zagreb",
                "2026-03-12",
                "Vlasnik vintage instrumenata",
                new List<Gitara>());

            _customers.AddRange(new[] { customerOne, customerTwo, customerThree });

            var guitarOne = new Gitara(1, "SN001", MarkeEnum.Fender, "6", TipGitareEnum.Elektricna, new DateTime(2026, 3, 22), 1);
            var guitarTwo = new Gitara(2, "SN002", MarkeEnum.Yamaha, "6", TipGitareEnum.Aukusticna, new DateTime(2026, 3, 27), 2);
            var guitarThree = new Gitara(3, "SN003", MarkeEnum.Gibson, "6", TipGitareEnum.Elektricna, new DateTime(2026, 4, 1), 3);
            var guitarFour = new Gitara(4, "SN004", MarkeEnum.Taylor, "12", TipGitareEnum.Aukusticna, new DateTime(2026, 4, 4), 1);

            _guitars.AddRange(new[] { guitarOne, guitarTwo, guitarThree, guitarFour });

            customerOne.Gitare.AddRange(new[] { guitarOne, guitarFour });
            customerTwo.Gitare.Add(guitarTwo);
            customerThree.Gitare.Add(guitarThree);

            _repairs.AddRange(new[]
            {
                new Nalog(1, guitarOne, customerOne, technicianOne, "Pukla zica", new DateTime(2026, 4, 8), new DateTime(2026, 4, 12), StatusNalogaEnum.Zaprimljen, VrstaPopravkaEnum.ZamjenaZica),
                new Nalog(2, guitarTwo, customerTwo, technicianOne, "Visok action", new DateTime(2026, 4, 10), new DateTime(2026, 4, 14), StatusNalogaEnum.UObradi, VrstaPopravkaEnum.PodesavanjeVrata),
                new Nalog(3, guitarThree, customerThree, technicianTwo, "Neispravna elektronika", new DateTime(2026, 4, 11), new DateTime(2026, 4, 18), StatusNalogaEnum.CekaDijelove, VrstaPopravkaEnum.PopravakElektronike),
                new Nalog(4, guitarFour, customerOne, technicianTwo, "Zamjena pragova", new DateTime(2026, 4, 13), new DateTime(2026, 4, 20), StatusNalogaEnum.Zavrsen, VrstaPopravkaEnum.ZamjenaPragova)
            });

            _office = new Poslovnica(
                _technicians,
                _managers,
                _repairs,
                _customers,
                "Servis Centar Zagreb",
                "Radnicka cesta 50, Zagreb");
        }

        public static IReadOnlyList<ZapTehnicar> Technicians => _technicians;
        public static IReadOnlyList<Zaposlenik> Managers => _managers;
        public static IReadOnlyList<Stranka> Customers => _customers;
        public static IReadOnlyList<Gitara> Guitars => _guitars;
        public static IReadOnlyList<Nalog> Repairs => _repairs;
        public static Poslovnica Office => _office;

        public static Stranka? FindCustomer(int id) => _customers.FirstOrDefault(customer => customer.Id == id);
        public static Gitara? FindGuitar(int id) => _guitars.FirstOrDefault(guitar => guitar.Id == id);
        public static ZapTehnicar? FindTechnician(int id) => _technicians.FirstOrDefault(technician => technician.Id == id);
        public static Nalog? FindRepair(int id) => _repairs.FirstOrDefault(repair => repair.Id == id);
    }
}