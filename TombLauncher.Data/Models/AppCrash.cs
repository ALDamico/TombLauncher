﻿namespace TombLauncher.Data.Models;

public class AppCrash
{
    public int Id { get; set; }
    public string Exception { get; set; }
    public DateTime DateTime { get; set; }
    public bool WasNotified { get; set; }
}