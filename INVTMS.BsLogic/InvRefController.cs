using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace INVTMS.BsLogic
{
    public class InvRefController
    {
        private readonly string _sqlConnection;

        public InvRefController(string sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        public List<InvReference> GetQueue()
        {
            var invRefObj = new List<InvReference>();
            var dsResult = new DataSet();

            using (SqlConnection conn = new SqlConnection(_sqlConnection))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT TOP 20 SHPTYPE, INVNO, DOCREF FROM INV4TMS ORDER BY DOCREF";
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;

                var da = new SqlDataAdapter(cmd);
                da.Fill(dsResult);
            }

            foreach (DataRow dr in dsResult.Tables[0].Rows)
            {
                invRefObj.Add(new InvReference()
                {
                    ShipmentType = dr["SHPTYPE"].ToString(),
                    InvoiceNo = dr["INVNO"].ToString(),
                    DocRef = dr["DOCREF"].ToString()
                });
            }

            return invRefObj;
        }

        public void Delete(InvReference invoice)
        {
            using (SqlConnection conn = new SqlConnection(_sqlConnection))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"DELETE INV4TMS WHERE INVNO=@INVNO AND DOCREF=@DOCREF AND SHPTYPE=@SHPTYPE";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@INVNO", invoice.InvoiceNo);
                cmd.Parameters.AddWithValue("@DOCREF", invoice.DocRef);
                cmd.Parameters.AddWithValue("@SHPTYPE", invoice.ShipmentType);
                cmd.ExecuteNonQuery();
            }
        }

        public List<CommercialInvoice> GetExpInvoice(InvReference invoice)
        {
            var dsResult = new DataSet();
            var sqlText = @"select InvNo AS COMMERCIAL_INV_NO, Item AS LINE_NO,
            (SELECT TOP 1 IHDES1 FROM HD010 S WHERE S.IHINVN=A.INVNO) AS COMMODITY_DESC,
            'DUMMY' AS CUSTOMER_CODE,
            (SELECT TOP 1 COMCD FROM HD010 S WHERE S.IHINVN=A.INVNO) AS CORP_CODE,
            (SELECT TOP 1 IHDEPT FROM HD010 S WHERE S.IHINVN=A.INVNO) AS DIVISION_ID,
            235 AS LOAD_LOCATION_ID, 235 AS DROP_LOCATION_ID,0 AS GROSS_WEIGHT,Package AS NUMBER_OF_PACKAGE,
            Unit AS UOM_PACKAGE, Hi AS PACK_HEIGHT, Le AS PACK_LENGTH, Wi AS PACK_WIDTH,
            ROUND(((Wi * Hi * Le)/1000000)* Package,2) AS VOLUME,
            DOCREF AS BATCH_NO, @SHPTYPE AS SHIPMENT_TYPE,
            (SELECT TOP 1 IHTRAD FROM HD010 S WHERE S.IHINVN=A.INVNO) AS SHIPMENT_MODE
            from HD010DIM A
            where INVNO=@INVNO and DocRef=@DOCREF";

            using (SqlConnection conn = new SqlConnection(_sqlConnection))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlText;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@SHPTYPE", invoice.ShipmentType);
                cmd.Parameters.AddWithValue("@INVNO", invoice.InvoiceNo);
                cmd.Parameters.AddWithValue("@DOCREF", invoice.DocRef);

                var da = new SqlDataAdapter(cmd);
                da.Fill(dsResult);
            }

            var commercialInvObj = new List<CommercialInvoice>();

            foreach (DataRow dr in dsResult.Tables[0].Rows)
            {
                commercialInvObj.Add(new CommercialInvoice()
                {
                    COMMERCIAL_INV_NO = dr["COMMERCIAL_INV_NO"].ToString(),
                    LINE_NO = Convert.ToInt32(dr["LINE_NO"]),
                    COMMODITY_DESC = dr["COMMODITY_DESC"].ToString(),
                    CUSTOMER_CODE = dr["CUSTOMER_CODE"].ToString(),
                    CORP_CODE = dr["CORP_CODE"].ToString(),
                    DIVISION_ID = dr["DIVISION_ID"].ToString(),
                    LOAD_LOCATION_ID = Convert.ToInt32(dr["LOAD_LOCATION_ID"]),
                    DROP_LOCATION_ID = Convert.ToInt32(dr["DROP_LOCATION_ID"]),
                    GROSS_WEIGHT = 0,
                    NUMBER_OF_PACKAGE = Convert.ToInt32(dr["NUMBER_OF_PACKAGE"]),
                    UOM_PACKAGE = dr["UOM_PACKAGE"].ToString(),
                    PACK_HEIGHT = Convert.ToDecimal(dr["PACK_HEIGHT"]),
                    PACK_LENGTH = Convert.ToDecimal(dr["PACK_LENGTH"]),
                    PACK_WIDTH = Convert.ToDecimal(dr["PACK_WIDTH"]),
                    VOLUME = Convert.ToDecimal(dr["VOLUME"]),
                    BATCH_NO = dr["BATCH_NO"].ToString(),
                    SHIPMENT_TYPE = dr["SHIPMENT_TYPE"].ToString(),
                    SHIPMENT_MODE = dr["SHIPMENT_MODE"].ToString()
                });
            }

            //decimal totalVolume = commercialInvObj.Sum(s => s.VOLUME).Value;
            //decimal totalGrossWeight = Convert.ToDecimal(dsResult.Tables[1].Rows[0][0]);

            //for (int i = 0; i < commercialInvObj.Count; i++)
            //{
            //    var cmcInv = commercialInvObj[i];

            //    if (cmcInv.VOLUME == 0 || totalVolume == 0)
            //    {
            //        cmcInv.GROSS_WEIGHT = totalGrossWeight;
            //    }
            //    else
            //        cmcInv.GROSS_WEIGHT = cmcInv.VOLUME * totalGrossWeight / totalVolume;
            //}

            return commercialInvObj;
        }

        public List<CommercialInvoice> GetImpInvoice(InvReference invoice)
        {
            var dsResult = new DataSet();
            var sqlText = @"select H5INVN AS COMMERCIAL_INV_NO, 1 AS LINE_NO,
                H5DESC AS COMMODITY_DESC,'DUMMY' AS CUSTOMER_CODE,COMCD AS CORP_CODE,
                H5DEPT AS DIVISION_ID, 235 AS LOAD_LOCATION_ID,235 AS DROP_LOCATION_ID, H5GWHT AS GROSS_WEIGHT,
                H5PACK AS NUMBER_OF_PACKAGE, H5PKUN AS UOM_PACKAGE, 0 AS PACK_HEIGHT, 0 AS PACK_LENGTH, 0 AS PACK_WIDTH,
                0 AS VOLUME, DOCREF AS BATCH_NO, @SHPTYPE AS SHIPMENT_TYPE, H5TRAD AS SHIPMENT_MODE
                from HD050 where H5INVN=@INVNO and DocRef=@DOCREF;";

            using (SqlConnection conn = new SqlConnection(_sqlConnection))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlText;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@SHPTYPE", invoice.ShipmentType);
                cmd.Parameters.AddWithValue("@INVNO", invoice.InvoiceNo);
                cmd.Parameters.AddWithValue("@DOCREF", invoice.DocRef);

                var da = new SqlDataAdapter(cmd);
                da.Fill(dsResult);
            }

            var commercialInvObj = new List<CommercialInvoice>();

            foreach (DataRow dr in dsResult.Tables[0].Rows)
            {
                commercialInvObj.Add(new CommercialInvoice()
                {
                    COMMERCIAL_INV_NO = dr["COMMERCIAL_INV_NO"].ToString(),
                    LINE_NO = Convert.ToInt32(dr["LINE_NO"]),
                    COMMODITY_DESC = dr["COMMODITY_DESC"].ToString(),
                    CUSTOMER_CODE = dr["CUSTOMER_CODE"].ToString(),
                    CORP_CODE = dr["CORP_CODE"].ToString(),
                    DIVISION_ID = dr["DIVISION_ID"].ToString(),
                    LOAD_LOCATION_ID = Convert.ToInt32(dr["LOAD_LOCATION_ID"]),
                    DROP_LOCATION_ID = Convert.ToInt32(dr["DROP_LOCATION_ID"]),
                    GROSS_WEIGHT = 0,
                    NUMBER_OF_PACKAGE = Convert.ToInt32(dr["NUMBER_OF_PACKAGE"]),
                    UOM_PACKAGE = dr["UOM_PACKAGE"].ToString(),
                    PACK_HEIGHT = Convert.ToDecimal(dr["PACK_HEIGHT"]),
                    PACK_LENGTH = Convert.ToDecimal(dr["PACK_LENGTH"]),
                    PACK_WIDTH = Convert.ToDecimal(dr["PACK_WIDTH"]),
                    VOLUME = Convert.ToDecimal(dr["VOLUME"]),
                    BATCH_NO = dr["BATCH_NO"].ToString(),
                    SHIPMENT_TYPE = dr["SHIPMENT_TYPE"].ToString(),
                    SHIPMENT_MODE = dr["SHIPMENT_MODE"].ToString()
                });
            }

            return commercialInvObj;
        }

        public byte[] ExportToBuffer(List<CommercialInvoice> invoices)
        {
            var invoiceJsonString = JsonConvert.SerializeObject(invoices, Formatting.Indented);

            var ms = new MemoryStream();
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry(RemoveSpecialChars(invoices[0].COMMERCIAL_INV_NO.Trim()) + ".json", invoiceJsonString);
                zip.Save(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms.GetBuffer();
        }

        private string RemoveSpecialChars(string input)
        {
            return Regex.Replace(input, @"[\./]", string.Empty);
        }
    }
}