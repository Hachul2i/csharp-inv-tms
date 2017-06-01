using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace INVTMS.BsLogic
{
    public class CommercialInvoiceController
    {
        private readonly string _connectionString;

        public CommercialInvoiceController(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<FileInfo> GetFiles(string location)
        {
            var dir = new DirectoryInfo(location);
            var files = dir.GetFiles("*.zip").OrderBy(o => o.LastAccessTime).ToList();

            if (files.Count == 0) return null;

            var fileObj = new List<FileInfo>();
            for (int i = 0; i < files.Count; i++)
            {
                fileObj.Add(files.ElementAt(i));
                if (i == 20) break;
            }

            return fileObj;
        }

        public List<CommercialInvoice> GetInvoice(string sourceFileName)
        {
            var buffer = ReadFileToObject(sourceFileName);
            var jsonString = System.Text.Encoding.UTF8.GetString(buffer);

            return JsonConvert.DeserializeObject<List<CommercialInvoice>>(jsonString);
        }

        private byte[] ReadFileToObject(string sourceFileName)
        {
            var ms = new MemoryStream();
            using (ZipFile zip = ZipFile.Read(sourceFileName))
            {
                var entries = zip.SelectEntries("name=*.json");
                if (entries.Count > 0)
                {
                    var ent = entries.ElementAt(0);
                    ent.Extract(ms);
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }

        public bool Insert(List<CommercialInvoice> invoices, out string errorString)
        {
            var result = false;
            errorString = string.Empty;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                var trans = conn.BeginTransaction();
                cmd.Transaction = trans;

                try
                {
                    var item = invoices.ElementAt(0);

                    // Delete before insert.
                    cmd.CommandText = @"DELETE FROM TBT_COMMERCIAL_INVOICE WHERE COMMERCIAL_INV_NO=@COMMERCIAL_INV_NO";
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@COMMERCIAL_INV_NO", item.COMMERCIAL_INV_NO);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();

                    cmd.CommandText = @"INSERT INTO TBT_COMMERCIAL_INVOICE(COMMERCIAL_INV_NO,LINE_NO,COMMODITY_DESC,
                    CUSTOMER_CODE,CORP_CODE,DIVISION_ID,LOAD_LOCATION_ID,DROP_LOCATION_ID,GROSS_WEIGHT,NUMBER_OF_PACKAGE,
                    UOM_PACKAGE,PACK_HEIGHT,PACK_LENGTH,PACK_WIDTH,VOLUME,BATCH_NO,SHIPMENT_TYPE,SHIPMENT_MODE)
                    VALUES (@COMMERCIAL_INV_NO,@LINE_NO,@COMMODITY_DESC,@CUSTOMER_CODE,@CORP_CODE,@DIVISION_ID,
                    @LOAD_LOCATION_ID,@DROP_LOCATION_ID,@GROSS_WEIGHT,@NUMBER_OF_PACKAGE,@UOM_PACKAGE,@PACK_HEIGHT,
                    @PACK_LENGTH,@PACK_WIDTH,@VOLUME,@BATCH_NO,@SHIPMENT_TYPE,@SHIPMENT_MODE)";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 0;

                    cmd.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@COMMERCIAL_INV_NO", SqlDbType.VarChar),
                        new SqlParameter("@LINE_NO", SqlDbType.Int),
                        new SqlParameter("@COMMODITY_DESC", SqlDbType.VarChar),
                        new SqlParameter("@CUSTOMER_CODE", SqlDbType.VarChar),
                        new SqlParameter("@CORP_CODE", SqlDbType.VarChar),
                        new SqlParameter("@DIVISION_ID", SqlDbType.VarChar),
                        new SqlParameter("@LOAD_LOCATION_ID", SqlDbType.Int),
                        new SqlParameter("@DROP_LOCATION_ID", SqlDbType.Int),
                        new SqlParameter("@GROSS_WEIGHT", SqlDbType.Decimal),
                        new SqlParameter("@NUMBER_OF_PACKAGE", SqlDbType.Int),
                        new SqlParameter("@UOM_PACKAGE", SqlDbType.VarChar),
                        new SqlParameter("@PACK_HEIGHT", SqlDbType.Decimal),
                        new SqlParameter("@PACK_LENGTH", SqlDbType.Decimal),
                        new SqlParameter("@PACK_WIDTH", SqlDbType.Decimal),
                        new SqlParameter("@VOLUME", SqlDbType.Decimal),
                        new SqlParameter("@BATCH_NO", SqlDbType.VarChar),
                        new SqlParameter("@SHIPMENT_TYPE", SqlDbType.VarChar),
                        new SqlParameter("@SHIPMENT_MODE", SqlDbType.VarChar)});

                    for (int i = 0; i < invoices.Count; i++)
                    {
                        var inv = invoices[i];

                        cmd.Parameters["@COMMERCIAL_INV_NO"].Value = inv.COMMERCIAL_INV_NO;
                        cmd.Parameters["@LINE_NO"].Value = inv.LINE_NO;
                        cmd.Parameters["@COMMODITY_DESC"].Value = inv.COMMODITY_DESC;
                        cmd.Parameters["@CUSTOMER_CODE"].Value = inv.CUSTOMER_CODE;
                        cmd.Parameters["@CORP_CODE"].Value = inv.CORP_CODE;
                        cmd.Parameters["@DIVISION_ID"].Value = inv.DIVISION_ID;
                        cmd.Parameters["@LOAD_LOCATION_ID"].Value = inv.LOAD_LOCATION_ID;
                        cmd.Parameters["@DROP_LOCATION_ID"].Value = inv.DROP_LOCATION_ID;
                        cmd.Parameters["@GROSS_WEIGHT"].Value = inv.GROSS_WEIGHT;
                        cmd.Parameters["@NUMBER_OF_PACKAGE"].Value = inv.NUMBER_OF_PACKAGE;
                        cmd.Parameters["@UOM_PACKAGE"].Value = inv.UOM_PACKAGE;
                        cmd.Parameters["@PACK_HEIGHT"].Value = inv.PACK_HEIGHT;
                        cmd.Parameters["@PACK_LENGTH"].Value = inv.PACK_LENGTH;
                        cmd.Parameters["@PACK_WIDTH"].Value = inv.PACK_WIDTH;
                        cmd.Parameters["@VOLUME"].Value = inv.VOLUME;
                        cmd.Parameters["@BATCH_NO"].Value = inv.BATCH_NO;
                        cmd.Parameters["@SHIPMENT_TYPE"].Value = inv.SHIPMENT_TYPE;
                        cmd.Parameters["@SHIPMENT_MODE"].Value = inv.SHIPMENT_MODE;

                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();

                    result = true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    errorString = ex.Message;
                }
            }

            return result;
        }
    }
}