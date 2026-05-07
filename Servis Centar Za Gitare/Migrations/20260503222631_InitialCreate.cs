using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Servis_Centar_Za_Gitare.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Poslovnice",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ime = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Adresa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Poslovnice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stranke",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PoslovnicaId = table.Column<long>(type: "bigint", nullable: true),
                    Ime = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Prezime = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BrojTelefona = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Adresa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DatumRegistracije = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Napomena = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stranke", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stranke_Poslovnice_PoslovnicaId",
                        column: x => x.PoslovnicaId,
                        principalTable: "Poslovnice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Zaposlenici",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PoslovnicaId = table.Column<long>(type: "bigint", nullable: true),
                    Ime = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Prezime = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BrojTelefona = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Adresa = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DatumZaposlenja = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Placa = table.Column<double>(type: "double precision", nullable: false),
                    TipZaposlenika = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zaposlenici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zaposlenici_Poslovnice_PoslovnicaId",
                        column: x => x.PoslovnicaId,
                        principalTable: "Poslovnice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Gitare",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SerijskiBroj = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Marka = table.Column<int>(type: "integer", nullable: false),
                    BrojZica = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    TipGitare = table.Column<int>(type: "integer", nullable: false),
                    DatumZaprimanja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    KupacId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gitare", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gitare_Stranke_KupacId",
                        column: x => x.KupacId,
                        principalTable: "Stranke",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Znanja",
                columns: table => new
                {
                    TehnicarId = table.Column<long>(type: "bigint", nullable: false),
                    TipGitare = table.Column<int>(type: "integer", nullable: false),
                    VrstaPopravka = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Znanja", x => new { x.TehnicarId, x.TipGitare, x.VrstaPopravka });
                    table.ForeignKey(
                        name: "FK_Znanja_Zaposlenici_TehnicarId",
                        column: x => x.TehnicarId,
                        principalTable: "Zaposlenici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Nalozi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GitaraId = table.Column<long>(type: "bigint", nullable: false),
                    StrankaId = table.Column<long>(type: "bigint", nullable: false),
                    TehnicarId = table.Column<long>(type: "bigint", nullable: false),
                    PoslovnicaId = table.Column<long>(type: "bigint", nullable: true),
                    OpisKvara = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DatumOtvaranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatumZatvaranja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VrstaPopravka = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nalozi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nalozi_Gitare_GitaraId",
                        column: x => x.GitaraId,
                        principalTable: "Gitare",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nalozi_Poslovnice_PoslovnicaId",
                        column: x => x.PoslovnicaId,
                        principalTable: "Poslovnice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Nalozi_Stranke_StrankaId",
                        column: x => x.StrankaId,
                        principalTable: "Stranke",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Nalozi_Zaposlenici_TehnicarId",
                        column: x => x.TehnicarId,
                        principalTable: "Zaposlenici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gitare_KupacId",
                table: "Gitare",
                column: "KupacId");

            migrationBuilder.CreateIndex(
                name: "IX_Nalozi_GitaraId",
                table: "Nalozi",
                column: "GitaraId");

            migrationBuilder.CreateIndex(
                name: "IX_Nalozi_PoslovnicaId",
                table: "Nalozi",
                column: "PoslovnicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nalozi_StrankaId",
                table: "Nalozi",
                column: "StrankaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nalozi_TehnicarId",
                table: "Nalozi",
                column: "TehnicarId");

            migrationBuilder.CreateIndex(
                name: "IX_Stranke_PoslovnicaId",
                table: "Stranke",
                column: "PoslovnicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Zaposlenici_PoslovnicaId",
                table: "Zaposlenici",
                column: "PoslovnicaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Nalozi");

            migrationBuilder.DropTable(
                name: "Znanja");

            migrationBuilder.DropTable(
                name: "Gitare");

            migrationBuilder.DropTable(
                name: "Zaposlenici");

            migrationBuilder.DropTable(
                name: "Stranke");

            migrationBuilder.DropTable(
                name: "Poslovnice");
        }
    }
}
