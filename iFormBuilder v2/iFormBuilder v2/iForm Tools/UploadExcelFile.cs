using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Excel;
using iFormBuilderAPI;

namespace iFormTools
{
    public class UploadExcelFile
    {
        IConfiguration iformconfig;
        iFormBuilder iformbuilder;
        public UploadExcelFile(IConfiguration config)
        {
            iformconfig = config;
            iformbuilder = new iFormBuilder(config);
        }

        public List<OptionList> CreateOptionList(string filepath)
        {
            List<OptionList> optionlist = new List<OptionList>();
            FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read);

            //1. Reading from a binary Excel file ('97-2003 format; *.xls)
            //IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            excelReader.IsFirstRowAsColumnNames = true;
            //3. DataSet - The result of each spreadsheet will be created in the result.Tables
            DataSet result = excelReader.AsDataSet();
            //4. DataSet - Create column names from first row
            excelReader.IsFirstRowAsColumnNames = true;

            int columns = result.Tables[0].Columns.Count;
            //Divide the Columns by 2 to create the List of OptionList
            int optListCount = columns / 2;
            OptionList optList = null;
            for (int i = 0; i < columns; i++)
            {
                if (i == 0 || UploadExcelFile.isEven(i))
                {
                    optList = new OptionList();
                    optList.NAME = result.Tables[0].Columns[i].ColumnName.Replace(" ", "_").ToLower();
                    optList.OPTIONS = new List<Option>();
                    optionlist.Add(optList);
                }
            }

            int optionkey = 0;
            foreach (DataRow dr in result.Tables[0].Rows)
            {
                for (int i = 0; i < columns; i++)
                {
                    Option opt = new Option();
                    if (i == 0)
                        optionkey = 0;
                    else
                        optionkey = i / 2;



                    opt.KEY_VALUE = dr.Field<object>(i).ToString();
                    i++;
                    opt.LABEL = dr.Field<object>(i).ToString();
                    if (i > 2)
                    {
                        //
                        opt.CONDITION_VALUE = string.Format("ZCDisplayValue_{0}=='{1}'", optionlist[optionkey - 1].NAME, dr.Field<string>(i - 2));
                    }

                    optionlist[optionkey].AddOption(opt);
                }
            }

            List<OptionList> test = iformbuilder.GetAllOptionLists();
            bool createlist = true;
            foreach (OptionList o in optionlist)
            {
                foreach (OptionList o1 in test)
                {
                    if (o.NAME == o1.NAME)
                    {
                        createlist = iformbuilder.DeleteOptionList(o1);
                        break;
                    }
                }

                if (createlist)
                    iformbuilder.CreateOptionList(o);
            }

            return optionlist;
        }

        public static Boolean isEven(int number)
        {
            return (number & 1) == 0;
        }
    }
}
