using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Servis_Centar_Za_Gitare.Migrations
{
    /// <inheritdoc />
    public partial class ConvertEnumsToLookupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VrstaPopravka",
                table: "Znanja",
                newName: "VrstaPopravkaId");

            migrationBuilder.RenameColumn(
                name: "TipGitare",
                table: "Znanja",
                newName: "TipGitareId");

            migrationBuilder.RenameColumn(
                name: "VrstaPopravka",
                table: "Nalozi",
                newName: "VrstaPopravkaId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Nalozi",
                newName: "StatusNalogaId");

            migrationBuilder.RenameColumn(
                name: "TipGitare",
                table: "Gitare",
                newName: "TipGitareId");

            migrationBuilder.RenameColumn(
                name: "Marka",
                table: "Gitare",
                newName: "MarkaId");

            migrationBuilder.CreateTable(
                name: "Marke",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marke", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusiNaloga",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusiNaloga", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoveGitara",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoveGitara", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VrstePopravke",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VrstePopravke", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Marke",
                columns: new[] { "Id", "Naziv" },
                values: new object[,]
                {
                    { 1, "Fender" },
                    { 2, "Gibson" },
                    { 3, "Yamaha" },
                    { 4, "Ibanez" },
                    { 5, "Taylor" },
                    { 6, "Martin" },
                    { 7, "PRS" },
                    { 8, "Epiphone" },
                    { 9, "Jackson" },
                    { 10, "Gretsch" },
                    { 11, "ESP" },
                    { 12, "Schecter" },
                    { 13, "Squier" },
                    { 14, "Takamine" },
                    { 15, "Charvel" }
                });

            migrationBuilder.InsertData(
                table: "StatusiNaloga",
                columns: new[] { "Id", "Naziv" },
                values: new object[,]
                {
                    { 1, "Zaprimljen" },
                    { 2, "U Obradi" },
                    { 3, "Čeka Dijelove" },
                    { 4, "Završen" },
                    { 5, "Otkazan" }
                });

            migrationBuilder.InsertData(
                table: "TipoveGitara",
                columns: new[] { "Id", "Naziv" },
                values: new object[,]
                {
                    { 1, "Aukusticna" },
                    { 2, "Elektricna" },
                    { 3, "Klasicna" },
                    { 4, "BasGitara" }
                });

            migrationBuilder.InsertData(
                table: "VrstePopravke",
                columns: new[] { "Id", "Naziv" },
                values: new object[,]
                {
                    { 1, "Zamjena Žica" },
                    { 2, "Podešavanje Vrata" },
                    { 3, "Podešavanje Intonacije" },
                    { 4, "Zamjena Pragova" },
                    { 5, "Brušenje Pragova" },
                    { 6, "Popravak Elektronike" },
                    { 7, "Zamjena Pickupa" },
                    { 8, "Zamjena Mašinica" },
                    { 9, "Popravak Kobilice" },
                    { 10, "Čišćenje Potenciometara" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Znanja_TipGitareId",
                table: "Znanja",
                column: "TipGitareId");

            migrationBuilder.CreateIndex(
                name: "IX_Znanja_VrstaPopravkaId",
                table: "Znanja",
                column: "VrstaPopravkaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nalozi_StatusNalogaId",
                table: "Nalozi",
                column: "StatusNalogaId");

            migrationBuilder.CreateIndex(
                name: "IX_Nalozi_VrstaPopravkaId",
                table: "Nalozi",
                column: "VrstaPopravkaId");

            migrationBuilder.CreateIndex(
                name: "IX_Gitare_MarkaId",
                table: "Gitare",
                column: "MarkaId");

            migrationBuilder.CreateIndex(
                name: "IX_Gitare_TipGitareId",
                table: "Gitare",
                column: "TipGitareId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gitare_Marke_MarkaId",
                table: "Gitare",
                column: "MarkaId",
                principalTable: "Marke",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gitare_TipoveGitara_TipGitareId",
                table: "Gitare",
                column: "TipGitareId",
                principalTable: "TipoveGitara",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nalozi_StatusiNaloga_StatusNalogaId",
                table: "Nalozi",
                column: "StatusNalogaId",
                principalTable: "StatusiNaloga",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nalozi_VrstePopravke_VrstaPopravkaId",
                table: "Nalozi",
                column: "VrstaPopravkaId",
                principalTable: "VrstePopravke",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Znanja_TipoveGitara_TipGitareId",
                table: "Znanja",
                column: "TipGitareId",
                principalTable: "TipoveGitara",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Znanja_VrstePopravke_VrstaPopravkaId",
                table: "Znanja",
                column: "VrstaPopravkaId",
                principalTable: "VrstePopravke",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gitare_Marke_MarkaId",
                table: "Gitare");

            migrationBuilder.DropForeignKey(
                name: "FK_Gitare_TipoveGitara_TipGitareId",
                table: "Gitare");

            migrationBuilder.DropForeignKey(
                name: "FK_Nalozi_StatusiNaloga_StatusNalogaId",
                table: "Nalozi");

            migrationBuilder.DropForeignKey(
                name: "FK_Nalozi_VrstePopravke_VrstaPopravkaId",
                table: "Nalozi");

            migrationBuilder.DropForeignKey(
                name: "FK_Znanja_TipoveGitara_TipGitareId",
                table: "Znanja");

            migrationBuilder.DropForeignKey(
                name: "FK_Znanja_VrstePopravke_VrstaPopravkaId",
                table: "Znanja");

            migrationBuilder.DropTable(
                name: "Marke");

            migrationBuilder.DropTable(
                name: "StatusiNaloga");

            migrationBuilder.DropTable(
                name: "TipoveGitara");

            migrationBuilder.DropTable(
                name: "VrstePopravke");

            migrationBuilder.DropIndex(
                name: "IX_Znanja_TipGitareId",
                table: "Znanja");

            migrationBuilder.DropIndex(
                name: "IX_Znanja_VrstaPopravkaId",
                table: "Znanja");

            migrationBuilder.DropIndex(
                name: "IX_Nalozi_StatusNalogaId",
                table: "Nalozi");

            migrationBuilder.DropIndex(
                name: "IX_Nalozi_VrstaPopravkaId",
                table: "Nalozi");

            migrationBuilder.DropIndex(
                name: "IX_Gitare_MarkaId",
                table: "Gitare");

            migrationBuilder.DropIndex(
                name: "IX_Gitare_TipGitareId",
                table: "Gitare");

            migrationBuilder.RenameColumn(
                name: "VrstaPopravkaId",
                table: "Znanja",
                newName: "VrstaPopravka");

            migrationBuilder.RenameColumn(
                name: "TipGitareId",
                table: "Znanja",
                newName: "TipGitare");

            migrationBuilder.RenameColumn(
                name: "VrstaPopravkaId",
                table: "Nalozi",
                newName: "VrstaPopravka");

            migrationBuilder.RenameColumn(
                name: "StatusNalogaId",
                table: "Nalozi",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "TipGitareId",
                table: "Gitare",
                newName: "TipGitare");

            migrationBuilder.RenameColumn(
                name: "MarkaId",
                table: "Gitare",
                newName: "Marka");
        }
    }
}
