using Microsoft.EntityFrameworkCore;
using MyEveryDay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyEveryDay
{
    public class RecordService
    {
        public static Task<List<int>> GetYears()
        {
            using var db = MyEveryDayDbContext.GetNew();
            return db.Records
                .Where(p=>!p.IsDeleted)
                .Select(p=>p.Year)
                .OrderBy(p=>p)
                .Distinct()
                .ToListAsync();
        }   
        public static Task<List<int>> GetMonthsAsync(int year)
        {
            using var db = MyEveryDayDbContext.GetNew();
            return db.Records
                .Where(p=>!p.IsDeleted)
                .Where(p=>p.Year == year)
                .Select(p=>p.Month)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }   
        public static Task<List<int>> GetDaysAsync(int year,int month)
        {
            using var db = MyEveryDayDbContext.GetNew();
            return db.Records
                .Where(p=>!p.IsDeleted)
                .Where(p=>p.Year == year)
                .Where(p=>p.Month == month)
                .Select(p=>p.Day)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public static async Task SaveAsync(int year,int month,int day,string rtf,string text)
        {
            var db = MyEveryDayDbContext.GetNew();
            var item =await db.Records
                .Where(p => p.Year == year)
                .Where(p => p.Month == month)
                .Where(p => p.Day == day)
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync();
            if(item==null)
            {
                item = new Record()
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    RichText = rtf,
                    PlainText = text
                };
                 db.Records.Add(item);
            }
            else
            {
                item.RichText = rtf;
                item.PlainText = text;
                db.Update(item);
            }
            await db.SaveChangesAsync();
        }

        public static async Task<string> GetRichTextAsync(int year,int month,int day)
        {
            var db = MyEveryDayDbContext.GetNew();
            var item =await db.Records
                .Where(p => p.Year == year)
                .Where(p => p.Month == month)
                .Where(p => p.Day == day)
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync();
            if(item==null)
            {
                throw new KeyNotFoundException($"没有找到{year}年{month}月{day}日的记录");
            }
            return item.RichText;
        }  
        public static async Task<List<Record>> GetRecordsAsync(int? year=null,int? month=null,int? day=null)
        {
            var db = MyEveryDayDbContext.GetNew();
            var records =await db.Records
                .Where(p => !year.HasValue || p.Year == year.Value)
                .Where(p => !month.HasValue || p.Month == month.Value)
                .Where(p => !day.HasValue || p.Month == day.Value)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month)
                .ThenBy(p => p.Day)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
            return records;
        }

       
    }
}
