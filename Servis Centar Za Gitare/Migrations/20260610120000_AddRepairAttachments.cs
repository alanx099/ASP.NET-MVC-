using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Servis_Centar_Za_Gitare.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NalogDatoteke",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NalogId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalniNaziv = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SpremljeniNaziv = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RelativnaPutanja = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipSadrzaja = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    VelicinaBajtova = table.Column<long>(type: "bigint", nullable: false),
                    DatumUploada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NalogDatoteke", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NalogDatoteke_Nalozi_NalogId",
                        column: x => x.NalogId,
                        principalTable: "Nalozi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NalogDatoteke_NalogId",
                table: "NalogDatoteke",
                column: "NalogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NalogDatoteke");
        }
    }
}
