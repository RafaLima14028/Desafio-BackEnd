using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

using DataAccess.Models;

public class DataBase
{
    private static DataBase database_instance;
    private string connectionUrl;

    private DataBase()
    {

    }

    public static DataBase GetInstanceDataBase()
    {
        if (database_instance == null)
            database_instance = new DataBase();

        return database_instance;
    }

    public bool PostMotocycle(Moto moto)
    {
        return true;
    }

    public bool GetMotocycle(string plate)
    {
        return true;
    }
}
