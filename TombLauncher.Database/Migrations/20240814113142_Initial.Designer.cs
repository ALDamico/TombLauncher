﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TombLauncher.Database;

#nullable disable

namespace TombLauncher.Database.Migrations
{
    [DbContext(typeof(TombLauncherDbContext))]
    [Migration("20240814113142_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("TombLauncher.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Author")
                        .HasColumnType("TEXT");

                    b.Property<int>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameEngine")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("InstallDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastPlayed")
                        .HasColumnType("TEXT");

                    b.Property<int>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Setting")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("TimePlayed")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });
#pragma warning restore 612, 618
        }
    }
}
