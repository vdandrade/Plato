﻿using System;
using System.Collections.Generic;


namespace Plato.Data.Migrations
{

    public enum MigrationType
    {
        Install,
        Upgrade,
        Rollback
    }
    
    public interface IDataMigrationBuilder
    {

        MigrationType DataMigrationType { get;  }

        IDataMigrationBuilder BuildMigrations(List<string> versions);

        DataMigrationResult ApplyMigrations();

    }
}
