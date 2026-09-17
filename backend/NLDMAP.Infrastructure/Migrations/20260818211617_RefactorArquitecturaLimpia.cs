using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLDMAP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorArquitecturaLimpia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_MissingPerson_PersonId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissingPerson",
                table: "MissingPerson");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "DisappearanceDate",
                table: "MissingPerson");

            migrationBuilder.RenameTable(
                name: "MissingPerson",
                newName: "MissingPersons");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "SsoProviderId");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Reports",
                newName: "Events_Longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "Reports",
                newName: "Events_Latitude");

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "Reports",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "LastSightingLocation_Timestamp",
                table: "Reports",
                newName: "Events_EventsDate");

            migrationBuilder.RenameColumn(
                name: "LastSightingLocation_GeoSource",
                table: "Reports",
                newName: "MissingPersonRelation");

            migrationBuilder.RenameColumn(
                name: "Folio",
                table: "Reports",
                newName: "Events_Street");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Reports",
                newName: "MissingPersonId");

            migrationBuilder.RenameColumn(
                name: "CaseStatus",
                table: "Reports",
                newName: "Events_State");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_PersonId",
                table: "Reports",
                newName: "IX_Reports_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "AlertRadioKm",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "BeAnonymous",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Curp",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Schooling",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SsoProvider",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ConsentUseExclusive",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Events_Circumstance",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Events_City",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Events_EventsHour",
                table: "Reports",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "Events_FactsDescription",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Events_Municipality",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Events_ReporterPresent",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MissingPersonId1",
                table: "Reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequestInformationPublic",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Reports",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Height",
                table: "MissingPersons",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Curp",
                table: "MissingPersons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "MissingPersons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDisability",
                table: "MissingPersons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IdentifyingSigns",
                table: "MissingPersons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "MissingPersons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "MissingPersons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotographyUrl",
                table: "MissingPersons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "MissingPersons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissingPersons",
                table: "MissingPersons",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_MissingPersonId",
                table: "Reports",
                column: "MissingPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_MissingPersonId1",
                table: "Reports",
                column: "MissingPersonId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_MissingPersons_MissingPersonId",
                table: "Reports",
                column: "MissingPersonId",
                principalTable: "MissingPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_MissingPersons_MissingPersonId1",
                table: "Reports",
                column: "MissingPersonId1",
                principalTable: "MissingPersons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_MissingPersons_MissingPersonId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_MissingPersons_MissingPersonId1",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_MissingPersonId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_MissingPersonId1",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissingPersons",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "BeAnonymous",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Curp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Schooling",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SsoProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConsentUseExclusive",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Events_Circumstance",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Events_City",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Events_EventsHour",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Events_FactsDescription",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Events_Municipality",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Events_ReporterPresent",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "MissingPersonId1",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "RequestInformationPublic",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Curp",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "HasDisability",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "IdentifyingSigns",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "PhotographyUrl",
                table: "MissingPersons");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "MissingPersons");

            migrationBuilder.RenameTable(
                name: "MissingPersons",
                newName: "MissingPerson");

            migrationBuilder.RenameColumn(
                name: "SsoProviderId",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "Events_Longitude",
                table: "Reports",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "Events_Latitude",
                table: "Reports",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reports",
                newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "MissingPersonRelation",
                table: "Reports",
                newName: "LastSightingLocation_GeoSource");

            migrationBuilder.RenameColumn(
                name: "MissingPersonId",
                table: "Reports",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "Events_Street",
                table: "Reports",
                newName: "Folio");

            migrationBuilder.RenameColumn(
                name: "Events_State",
                table: "Reports",
                newName: "CaseStatus");

            migrationBuilder.RenameColumn(
                name: "Events_EventsDate",
                table: "Reports",
                newName: "LastSightingLocation_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                newName: "IX_Reports_PersonId");

            migrationBuilder.AlterColumn<string>(
                name: "AlertRadioKm",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Reports",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Height",
                table: "MissingPerson",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisappearanceDate",
                table: "MissingPerson",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissingPerson",
                table: "MissingPerson",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_MissingPerson_PersonId",
                table: "Reports",
                column: "PersonId",
                principalTable: "MissingPerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
