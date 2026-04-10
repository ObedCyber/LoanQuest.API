using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanRequestInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_DocumentId_docRequirement_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LoanDocumentId",
                table: "ApplicationDocumentChecklists",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoanDocumentId",
                table: "ApplicationDocumentChecklists");
        }
    }
}
