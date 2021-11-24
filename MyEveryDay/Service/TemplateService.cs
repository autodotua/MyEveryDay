using Microsoft.EntityFrameworkCore;
using MyEveryDay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MyEveryDay
{
    public static class TemplateService
    {
        public static readonly string YearTitle
      = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Microsoft YaHei;}{\f2\fcharset0 Microsoft YaHei;}{\f3\fcharset0 Microsoft YaHei;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs48\f3\cf0 \cf0\qj{\f3 {\lang2052\ltrch %Year%\u24180?}\li0\ri0\sa200\sb0\fi0\ql\par}
}
}";
        public static readonly string MonthTitle
        = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Microsoft YaHei;}{\f2\fcharset0 Microsoft YaHei;}{\f3\fcharset0 Microsoft YaHei;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs40\f3\cf0 \cf0\qj{\f3 {\lang2052\ltrch %Month%\u26376?}\li0\ri0\sa200\sb0\fi0\ql\par}
}
}";
        public static readonly string DayTitle
        = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Microsoft YaHei;}{\f2\fcharset0 Microsoft YaHei;}{\f3\fcharset0 Microsoft YaHei;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs27\f3\cf0 \cf0\qj{\f3 {\lang2052\ltrch %Day%\u26085?}\li0\ri0\sa200\sb0\fi0\ql\par}
}
}";
        private static async Task<string> GetDateTitleAsync(string name)
        {
            using var db = MyEveryDayDbContext.GetNew();
            return (await db.Templates.FirstOrDefaultAsync(p =>
            p.Type == TemplateType.DateTitle && p.Name == name))?.RichText;
        }
        public static async Task<string> GetDayTitleAsync()
        {
            return await GetDateTitleAsync("Day") ?? DayTitle;
        }
        public static async Task<string> GetMonthTitleAsync()
        {
            return await GetDateTitleAsync("Month") ?? MonthTitle;
        }
        public static async Task<string> GetYearTitleAsync()
        {
            return await GetDateTitleAsync("Year") ?? YearTitle;
        }

        private static async Task UpdateDateTitle(string name, string text)
        {
            using var db = MyEveryDayDbContext.GetNew();
            var template = await db.Templates.FirstOrDefaultAsync(p => p.Type == TemplateType.DateTitle && p.Name == name);
            if (template == null)
            {
                template = new Template()
                {
                    Name = name,
                    RichText = text,
                    Type = TemplateType.DateTitle,
                };
                db.Templates.Add(template);
                await db.SaveChangesAsync();
            }
            else
            {
                template.RichText = text;
                db.Update(template);
                await db.SaveChangesAsync();
            }

        }
        public static async Task UpdateDayTitle(string text)
        {
            await UpdateDateTitle("Day", text);
        }
        public static async Task UpdateMonthTitle(string text)
        {
            await UpdateDateTitle("Month", text);
        }
        public static async Task UpdateYearTitle(string text)
        {
            await UpdateDateTitle("Year", text);
        }

        public static async Task<List<Template>> GetArticleTemplatesAsync()
        {
            using var db = MyEveryDayDbContext.GetNew();
            return await db.Templates.Where(p => p.IsDeleted == false && p.Type == TemplateType.Article).ToListAsync();
        }

        public static async Task<Template> AddArticleTemplateAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            using var db = MyEveryDayDbContext.GetNew();
            if (db.Templates.Any(p => p.Name == name && p.IsDeleted == false && p.Type == TemplateType.Article))
            {
                throw new Exception("模板已存在");
            }
            var template = new Template()
            {
                Type = TemplateType.Article,
                Name = name
            };
            db.Templates.Add(template);
            await db.SaveChangesAsync();
            return template;
        }
        public static async Task UpdateArticleTemplateAsync(int id, string text)
        {

            using var db = MyEveryDayDbContext.GetNew();
            var template = await db.Templates.FindAsync(id);
            if (template == null)
            {
                throw new KeyNotFoundException("数据库中找不到指定的模板");
            }
            template.RichText = text;
            db.Update(template);

            await db.SaveChangesAsync();
        }
        public static async Task DeleteArticleTemplateAsync(int id)
        {
            using var db = MyEveryDayDbContext.GetNew();
            var template = await db.Templates.FindAsync(id);
            if (template == null)
            {
                throw new KeyNotFoundException("数据库中找不到指定的模板");
            }
            db.Templates.Remove(template);
            await db.SaveChangesAsync();
        }
    }
}
