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
    [Migration("20240901181416_AddGameLinks")]
    partial class AddGameLinks
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("TombLauncher.Models.AppCrash", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Exception")
                        .HasColumnType("TEXT");

                    b.Property<bool>("WasNotified")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AppCrashes");
                });

            modelBuilder.Entity("TombLauncher.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Author")
                        .HasColumnType("TEXT");

                    b.Property<string>("AuthorFullName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ExecutablePath")
                        .HasColumnType("TEXT");

                    b.Property<int>("GameEngine")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("Guid")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("InstallDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("InstallDirectory")
                        .HasColumnType("TEXT");

                    b.Property<int>("Length")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Setting")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("TitlePic")
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("TombLauncher.Models.GameHashes", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Md5Hash")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("GameHashes");
                });

            modelBuilder.Entity("TombLauncher.Models.GameLink", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Link")
                        .HasColumnType("TEXT");

                    b.Property<int>("LinkType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("GameLink");
                });

            modelBuilder.Entity("TombLauncher.Models.PlaySession", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("PlaySession");
                });

            modelBuilder.Entity("TombLauncher.Models.GameHashes", b =>
                {
                    b.HasOne("TombLauncher.Models.Game", null)
                        .WithMany("Hashes")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TombLauncher.Models.GameLink", b =>
                {
                    b.HasOne("TombLauncher.Models.Game", null)
                        .WithMany("Links")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TombLauncher.Models.PlaySession", b =>
                {
                    b.HasOne("TombLauncher.Models.Game", "Game")
                        .WithMany("PlaySessions")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("TombLauncher.Models.Game", b =>
                {
                    b.Navigation("Hashes");

                    b.Navigation("Links");

                    b.Navigation("PlaySessions");
                });
#pragma warning restore 612, 618
        }
    }
}
