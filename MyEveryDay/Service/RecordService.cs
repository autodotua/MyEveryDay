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
        public static Task<List<int>> GetMonths(int year)
        {
            using var db = MyEveryDayDbContext.GetNew();
            return db.Records
                .Where(p=>!p.IsDeleted)
                .Where(p=>p.Year == year)
                .Select(p=>p.Month)
                .OrderBy(p=>p)
                .Distinct()
                .ToListAsync();
        }   
        public static Task<List<int>> GetDays(int year,int month)
        {
            using var db = MyEveryDayDbContext.GetNew();
            return db.Records
                .Where(p=>!p.IsDeleted)
                .Where(p=>p.Year == year)
                .Where(p=>p.Month == month)
                .Select(p=>p.Day)
                .OrderBy(p=>p)
                .Distinct()
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
                await db.Records.AddAsync(item);
            }
            else
            {
                item.RichText = rtf;
                item.PlainText = text;
                db.Entry(item).State = EntityState.Modified;
            }
            await db.SaveChangesAsync();
        }

        public static async Task<string> GetRichText(int year,int month,int day)
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
        public static async Task<List<Record>> Get(int year,int month)
        {
            var db = MyEveryDayDbContext.GetNew();
            var records =await db.Records
                .Where(p => p.Year == year)
                .Where(p => p.Month == month)
                .OrderBy(p=>p.Day)
                .Where(p => !p.IsDeleted)
                .ToListAsync   ();
            return records;
        }
    }
}
