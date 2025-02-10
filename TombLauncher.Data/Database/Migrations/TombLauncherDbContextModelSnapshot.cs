﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TombLauncher.Data.Database;

#nullable disable

namespace TombLauncher.Data.Database.Migrations
{
    [DbContext(typeof(TombLauncherDbContext))]
    partial class TombLauncherDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("TombLauncher.Data.Models.AppCrash", b =>
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

            modelBuilder.Entity("TombLauncher.Data.Models.FileBackup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Arguments")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("BackedUpOn")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .HasColumnType("BLOB");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.Property<int>("FileType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Md5")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameId");

                    b.ToTable("FileBackups");
                });

            modelBuilder.Entity("TombLauncher.Data.Models.Game", b =>
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

                    b.Property<bool>("IsInstalled")
                        .HasColumnType("INTEGER");

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

                    b.Property<string>("UniversalLauncherPath")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("TombLauncher.Data.Models.GameHashes", b =>
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

            modelBuilder.Entity("TombLauncher.Data.Models.GameLink", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("BaseUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

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

            modelBuilder.Entity("TombLauncher.Data.Models.PlaySession", b =>
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

            modelBuilder.Entity("TombLauncher.Data.Models.SavegameMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("FileBackupId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LevelName")
                        .HasColumnType("TEXT");

                    b.Property<int>("SaveNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SlotNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("FileBackupId")
                        .IsUnique();

                    b.ToTable("SavegameMetadata");
                });

            modelBuilder.Entity("TombLauncher.Data.Models.FileBackup", b =>
                {
                    b.HasOne("TombLauncher.Data.Models.Game", null)
                        .WithMany("FileBackups")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TombLauncher.Data.Models.GameHashes", b =>
                {
                    b.HasOne("TombLauncher.Data.Models.Game", null)
                        .WithMany("Hashes")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TombLauncher.Data.Models.GameLink", b =>
                {
                    b.HasOne("TombLauncher.Data.Models.Game", null)
                        .WithMany("Links")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TombLauncher.Data.Models.PlaySession", b =>
                {
                    b.HasOne("TombLauncher.Data.Models.Game", "Game")
                        .WithMany("PlaySessions")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Game");
                });

            modelBuilder.Entity("TombLauncher.Data.Models.SavegameMetadata", b =>
                {
                    b.HasOne("TombLauncher.Data.Models.FileBackup", "FileBackup")
                        .WithOne("SavegameMetadata")
                        .HasForeignKey("TombLauncher.Data.Models.SavegameMetadata", "FileBackupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FileBackup");
                });

            modelBuilder.Entity("TombLauncher.Data.Models.FileBackup", b =>
                {
                    b.Navigation("SavegameMetadata");
                });

            modelBuilder.Entity("TombLauncher.Data.Models.Game", b =>
                {
                    b.Navigation("FileBackups");

                    b.Navigation("Hashes");

                    b.Navigation("Links");

                    b.Navigation("PlaySessions");
                });
#pragma warning restore 612, 618
        }
    }
}
