﻿// <auto-generated />
using DIGNDB.App.SmitteStop.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace DIGNDB.App.SmitteStop.DAL.Migrations
{
    [DbContext(typeof(DigNDB_SmittestopContext))]
    [Migration("20200903135204_AddCountryAndOrigin")]
    partial class AddCountryAndOrigin
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DIGNDB.App.SmitteStop.Core.Models.Country", b =>
            {
                b.Property<long>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                b.Property<string>("Code")
                    .HasColumnType("nvarchar(max)");

                b.Property<bool>("IsGatewayEnabled")
                    .HasColumnType("bit");

                b.Property<string>("Name")
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("Country");

                b.HasData(
                    new
                    {
                        Id = 1L,
                        Code = "AT",
                        IsGatewayEnabled = false,
                        Name = "Østrig"
                    },
                    new
                    {
                        Id = 2L,
                        Code = "BE",
                        IsGatewayEnabled = false,
                        Name = "Belgien"
                    },
                    new
                    {
                        Id = 3L,
                        Code = "BG",
                        IsGatewayEnabled = false,
                        Name = "Bulgarien"
                    },
                    new
                    {
                        Id = 4L,
                        Code = "HR",
                        IsGatewayEnabled = false,
                        Name = "Kroatien"
                    },
                    new
                    {
                        Id = 5L,
                        Code = "CY",
                        IsGatewayEnabled = false,
                        Name = "Cypern"
                    },
                    new
                    {
                        Id = 6L,
                        Code = "CZ",
                        IsGatewayEnabled = false,
                        Name = "Tjekkiet"
                    },
                    new
                    {
                        Id = 7L,
                        Code = "DK",
                        IsGatewayEnabled = false,
                        Name = "Danmark"
                    },
                    new
                    {
                        Id = 8L,
                        Code = "EE",
                        IsGatewayEnabled = false,
                        Name = "Estland"
                    },
                    new
                    {
                        Id = 9L,
                        Code = "FI",
                        IsGatewayEnabled = false,
                        Name = "Finland"
                    },
                    new
                    {
                        Id = 10L,
                        Code = "FR",
                        IsGatewayEnabled = false,
                        Name = "Frankrig"
                    },
                    new
                    {
                        Id = 11L,
                        Code = "DE",
                        IsGatewayEnabled = false,
                        Name = "Tyskland"
                    },
                    new
                    {
                        Id = 12L,
                        Code = "GR",
                        IsGatewayEnabled = false,
                        Name = "Grækenland"
                    },
                    new
                    {
                        Id = 13L,
                        Code = "HU",
                        IsGatewayEnabled = false,
                        Name = "Ungarn"
                    },
                    new
                    {
                        Id = 14L,
                        Code = "IE",
                        IsGatewayEnabled = false,
                        Name = "Irland"
                    },
                    new
                    {
                        Id = 15L,
                        Code = "IT",
                        IsGatewayEnabled = false,
                        Name = "Italien"
                    },
                    new
                    {
                        Id = 16L,
                        Code = "LV",
                        IsGatewayEnabled = false,
                        Name = "Letland"
                    },
                    new
                    {
                        Id = 17L,
                        Code = "LT",
                        IsGatewayEnabled = false,
                        Name = "Litauen"
                    },
                    new
                    {
                        Id = 18L,
                        Code = "LU",
                        IsGatewayEnabled = false,
                        Name = "Luxembourg"
                    },
                    new
                    {
                        Id = 19L,
                        Code = "MT",
                        IsGatewayEnabled = false,
                        Name = "Malta"
                    },
                    new
                    {
                        Id = 20L,
                        Code = "NL",
                        IsGatewayEnabled = false,
                        Name = "Holland"
                    },
                    new
                    {
                        Id = 21L,
                        Code = "PL",
                        IsGatewayEnabled = false,
                        Name = "Polen"
                    },
                    new
                    {
                        Id = 22L,
                        Code = "PT",
                        IsGatewayEnabled = false,
                        Name = "Portugal"
                    },
                    new
                    {
                        Id = 23L,
                        Code = "RO",
                        IsGatewayEnabled = false,
                        Name = "Rumænien"
                    },
                    new
                    {
                        Id = 24L,
                        Code = "SK",
                        IsGatewayEnabled = false,
                        Name = "Slovakiet"
                    },
                    new
                    {
                        Id = 25L,
                        Code = "SI",
                        IsGatewayEnabled = false,
                        Name = "Slovenien"
                    },
                    new
                    {
                        Id = 26L,
                        Code = "ES",
                        IsGatewayEnabled = false,
                        Name = "Spanien"
                    },
                    new
                    {
                        Id = 27L,
                        Code = "SE",
                        IsGatewayEnabled = false,
                        Name = "Sverige"
                    });
            });

            modelBuilder.Entity("DIGNDB.App.SmitteStop.Core.Models.TemporaryExposureKey", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnName("ID")
                    .HasColumnType("uniqueidentifier")
                    .HasDefaultValueSql("(newid())");

                b.Property<DateTime>("CreatedOn")
                    .HasColumnType("datetime");

                b.Property<byte[]>("KeyData")
                    .IsRequired()
                    .HasColumnType("varbinary(255)")
                    .HasMaxLength(255);

                b.Property<long>("OriginId")
                    .HasColumnType("bigint");

                b.Property<long>("RollingPeriod")
                    .HasColumnType("bigint");

                b.Property<long>("RollingStartNumber")
                    .HasColumnType("bigint");

                b.Property<bool>("SendToGateway")
                    .HasColumnType("bit");

                b.Property<int>("TransmissionRiskLevel")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("OriginId");

                b.ToTable("TemporaryExposureKey");
            });

            modelBuilder.Entity("DIGNDB.App.SmitteStop.Core.Models.TemporaryExposureKeyCountry", b =>
            {
                b.Property<Guid>("TemporaryExposureKeyId")
                    .HasColumnType("uniqueidentifier");

                b.Property<long>("CountryId")
                    .HasColumnType("bigint");

                b.HasKey("TemporaryExposureKeyId", "CountryId");

                b.HasIndex("CountryId");

                b.ToTable("TemporaryExposureKeyCountry");
            });

            modelBuilder.Entity("DIGNDB.App.SmitteStop.Core.Models.TemporaryExposureKey", b =>
            {
                b.HasOne("DIGNDB.App.SmitteStop.Core.Models.Country", "Origin")
                    .WithMany()
                    .HasForeignKey("OriginId")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
            });

            modelBuilder.Entity("DIGNDB.App.SmitteStop.Core.Models.TemporaryExposureKeyCountry", b =>
            {
                b.HasOne("DIGNDB.App.SmitteStop.Core.Models.Country", "Country")
                    .WithMany("TemporaryExposureKeyCountries")
                    .HasForeignKey("CountryId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("DIGNDB.App.SmitteStop.Core.Models.TemporaryExposureKey", "TemporaryExposureKey")
                    .WithMany("VisitedCountries")
                    .HasForeignKey("TemporaryExposureKeyId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
#pragma warning restore 612, 618
        }
    }
}