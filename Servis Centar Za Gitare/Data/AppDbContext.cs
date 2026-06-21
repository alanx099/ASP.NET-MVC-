using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data
{
    public class AppDbContext: IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Gitara> Gitare => Set<Gitara>();
        public DbSet<Nalog> Nalozi => Set<Nalog>();
        public DbSet<Poslovnica> Poslovnice => Set<Poslovnica>();
        public DbSet<Stranka> Stranke => Set<Stranka>();
        public DbSet<Zaposlenik> Zaposlenici => Set<Zaposlenik>();
        public DbSet<ZapTehnicar> Tehnicari => Set<ZapTehnicar>();
        public DbSet<Znanje> Znanja => Set<Znanje>();
        public DbSet<NalogDatoteka> NalogDatoteke => Set<NalogDatoteka>();
        public DbSet<Marka> Marke => Set<Marka>();
        public DbSet<TipGitare> TipoveGitara => Set<TipGitare>();
        public DbSet<StatusNaloga> StatusiNaloga => Set<StatusNaloga>();
        public DbSet<VrstaPopravka> VrstePopravke => Set<VrstaPopravka>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names for lookup tables (explicit mapping to match migration names)
            modelBuilder.Entity<VrstaPopravka>().ToTable("VrstePopravke");
            modelBuilder.Entity<StatusNaloga>().ToTable("StatusiNaloga");
            modelBuilder.Entity<TipGitare>().ToTable("TipoveGitara");
            modelBuilder.Entity<Marka>().ToTable("Marke");

            // Seed Lookup Tables
            modelBuilder.Entity<Marka>().HasData(
                new Marka { Id = 1, Naziv = "Fender" },
                new Marka { Id = 2, Naziv = "Gibson" },
                new Marka { Id = 3, Naziv = "Yamaha" },
                new Marka { Id = 4, Naziv = "Ibanez" },
                new Marka { Id = 5, Naziv = "Taylor" },
                new Marka { Id = 6, Naziv = "Martin" },
                new Marka { Id = 7, Naziv = "PRS" },
                new Marka { Id = 8, Naziv = "Epiphone" },
                new Marka { Id = 9, Naziv = "Jackson" },
                new Marka { Id = 10, Naziv = "Gretsch" },
                new Marka { Id = 11, Naziv = "ESP" },
                new Marka { Id = 12, Naziv = "Schecter" },
                new Marka { Id = 13, Naziv = "Squier" },
                new Marka { Id = 14, Naziv = "Takamine" },
                new Marka { Id = 15, Naziv = "Charvel" }
            );

            modelBuilder.Entity<TipGitare>().HasData(
                new TipGitare { Id = 1, Naziv = "Aukusticna" },
                new TipGitare { Id = 2, Naziv = "Elektricna" },
                new TipGitare { Id = 3, Naziv = "Klasicna" },
                new TipGitare { Id = 4, Naziv = "BasGitara" }
            );

            modelBuilder.Entity<StatusNaloga>().HasData(
                new StatusNaloga { Id = 1, Naziv = "Zaprimljen" },
                new StatusNaloga { Id = 2, Naziv = "U Obradi" },
                new StatusNaloga { Id = 3, Naziv = "Čeka Dijelove" },
                new StatusNaloga { Id = 4, Naziv = "Završen" },
                new StatusNaloga { Id = 5, Naziv = "Otkazan" }
            );

            modelBuilder.Entity<VrstaPopravka>().HasData(
                new VrstaPopravka { Id = 1, Naziv = "Zamjena Žica" },
                new VrstaPopravka { Id = 2, Naziv = "Podešavanje Vrata" },
                new VrstaPopravka { Id = 3, Naziv = "Podešavanje Intonacije" },
                new VrstaPopravka { Id = 4, Naziv = "Zamjena Pragova" },
                new VrstaPopravka { Id = 5, Naziv = "Brušenje Pragova" },
                new VrstaPopravka { Id = 6, Naziv = "Popravak Elektronike" },
                new VrstaPopravka { Id = 7, Naziv = "Zamjena Pickupa" },
                new VrstaPopravka { Id = 8, Naziv = "Zamjena Mašinica" },
                new VrstaPopravka { Id = 9, Naziv = "Popravak Kobilice" },
                new VrstaPopravka { Id = 10, Naziv = "Čišćenje Potenciometara" }
            );

            modelBuilder.Entity<Zaposlenik>()
                .HasDiscriminator<string>("TipZaposlenika")
                .HasValue<Zaposlenik>("Zaposlenik")
                .HasValue<ZapTehnicar>("ZapTehnicar");

            // Gitara relationships
            modelBuilder.Entity<Gitara>()
                .HasOne(g => g.Marka)
                .WithMany(m => m.Gitare)
                .HasForeignKey(g => g.MarkaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Gitara>()
                .HasOne(g => g.TipGitare)
                .WithMany(t => t.Gitare)
                .HasForeignKey(g => g.TipGitareId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Gitara>()
                .HasOne(g => g.Kupac)
                .WithMany(s => s.Gitare)
                .HasForeignKey(g => g.KupacId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stranka>()
                .HasOne(s => s.Poslovnica)
                .WithMany(p => p.Stranke)
                .HasForeignKey(s => s.PoslovnicaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Stranka>()
                .HasOne(s => s.AppUser)
                .WithOne(u => u.Stranka)
                .HasForeignKey<Stranka>(s => s.AppUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Stranka>()
                .HasIndex(s => s.AppUserId)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(token => token.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Zaposlenik>()
                .HasOne(z => z.Poslovnica)
                .WithMany(p => p.Zaposlenici)
                .HasForeignKey(z => z.PoslovnicaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Nalog relationships
            modelBuilder.Entity<Nalog>()
                .HasOne(n => n.Gitara)
                .WithMany(g => g.Nalozi)
                .HasForeignKey(n => n.GitaraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Nalog>()
                .HasOne(n => n.Stranka)
                .WithMany(s => s.Nalozi)
                .HasForeignKey(n => n.StrankaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Nalog>()
                .HasOne(n => n.Tehnicar)
                .WithMany(t => t.Nalozi)
                .HasForeignKey(n => n.TehnicarId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Nalog>()
                .HasOne(n => n.Poslovnica)
                .WithMany(p => p.Nalozi)
                .HasForeignKey(n => n.PoslovnicaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Nalog>()
                .HasOne(n => n.StatusNaloga)
                .WithMany(s => s.Nalozi)
                .HasForeignKey(n => n.StatusNalogaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Nalog>()
                .HasOne(n => n.VrstaPopravka)
                .WithMany(v => v.Nalozi)
                .HasForeignKey(n => n.VrstaPopravkaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NalogDatoteka>()
                .HasOne(d => d.Nalog)
                .WithMany(n => n.Datoteke)
                .HasForeignKey(d => d.NalogId)
                .OnDelete(DeleteBehavior.Cascade);

            // Znanje relationships and key
            modelBuilder.Entity<Znanje>()
                .HasKey(z => new { z.TehnicarId, z.TipGitareId, z.VrstaPopravkaId });

            modelBuilder.Entity<Znanje>()
                .HasOne(z => z.Tehnicar)
                .WithMany(t => t.Znanja)
                .HasForeignKey(z => z.TehnicarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Znanje>()
                .HasOne(z => z.TipGitare)
                .WithMany(t => t.Znanja)
                .HasForeignKey(z => z.TipGitareId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Znanje>()
                .HasOne(z => z.VrstaPopravka)
                .WithMany(v => v.Znanja)
                .HasForeignKey(z => z.VrstaPopravkaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
