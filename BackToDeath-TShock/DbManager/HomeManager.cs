﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace BackToDeath_TShock.DbManager
{
    public class HomeManager
    {
        private IDbConnection db;
        private List<Home> homes = new List<Home>();
        private object syncLock = new object();

        public HomeManager(IDbConnection db)
        {
            this.db = db;

            var sqlCreator = new SqlTableCreator(db, (IQueryBuilder)new SqliteQueryCreator());
            sqlCreator.EnsureTableStructure(new SqlTable("Homes",
                new SqlColumn("ID", MySqlDbType.Int32) { AutoIncrement = true, Primary = true },
                new SqlColumn("UUID", MySqlDbType.Text),
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("X", MySqlDbType.Double),
                new SqlColumn("Y", MySqlDbType.Double),
                new SqlColumn("WorldID", MySqlDbType.Int32)));

            using (QueryResult result = db.QueryReader("SELECT * FROM Homes WHERE WorldID = @0", Main.worldID))
            {
                while (result.Read())
                {
                    homes.Add(new Home(
                        result.Get<string>("UUID"),
                        result.Get<string>("Name"),
                        result.Get<float>("X"),
                        result.Get<float>("Y")));
                }
            }
        }

        public async Task<bool> AddAsync(TSPlayer player, string name, float x, float y)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        homes.Add(new Home(player.UUID, name, x, y));
                        return db.Query("INSERT INTO Homes (UUID, Name, X, Y, WorldID) VALUES (@0, @1, @2, @3, @4)",
                            player.UUID,
                            name,
                            x,
                            y,
                            Main.worldID) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<bool> DeleteAsync(TSPlayer player, string name)
        {
            string query = db.GetSqlType() == SqlType.Mysql
                ? "DELETE FROM Homes WHERE UUID = @0 AND Name = @1 AND WorldID = @2"
                : "DELETE FROM Homes WHERE UUID = @0 AND Name = @1 AND WorldID = @2 COLLATE NOCASE";

            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        homes.RemoveAll(h => h.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                            && h.UUID == player.UUID);
                        return db.Query(query, player.UUID, name, Main.worldID) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<Home> GetAsync(TSPlayer player, string name)
        {
            return await Task.Run(() =>
            {
                lock (syncLock)
                {
                    return
                        homes.Find(h =>
                            h.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                            && h.UUID == player.UUID);
                }
            });
        }

        public async Task<List<Home>> GetAllAsync(TSPlayer player)
        {
            return await Task.Run(() =>
            {
                lock (syncLock)
                {
                    return homes.FindAll(h => h.UUID == player.UUID);
                }
            });
        }

        public async Task<bool> ReloadAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        homes.Clear();
                        using (QueryResult result = db.QueryReader("SELECT * FROM Homes WHERE WorldID = @0", Main.worldID))
                        {
                            while (result.Read())
                            {
                                homes.Add(new Home(
                                    result.Get<string>("UUID"),
                                    result.Get<string>("Name"),
                                    result.Get<float>("X"),
                                    result.Get<float>("Y")));
                            }
                        }
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }

        public async Task<bool> UpdateAsync(TSPlayer player, string name, float x, float y)
        {
            string query = db.GetSqlType() == SqlType.Mysql
                ? "UPDATE Homes SET X = @0, Y = @1 WHERE UUID = @2 AND Name = @3 AND WorldID = @4"
                : "UPDATE Homes SET X = @0, Y = @1 WHERE UUID = @2 AND Name = @3 AND WorldID = @4 COLLATE NOCASE";

            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        homes.RemoveAll(h => h.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                            && h.UUID == player.UUID);
                        homes.Add(new Home(player.UUID, name, x, y));
                        return db.Query(query, x, y, player.UUID, name, Main.worldID) > 0;
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return false;
                }
            });
        }
    }
}
