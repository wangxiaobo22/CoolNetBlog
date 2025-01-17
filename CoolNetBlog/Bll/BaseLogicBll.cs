﻿using ComponentsServices.Base;
using CoolNetBlog.Models;
using CoolNetBlog.ViewModels.Admin;
using CoolNetBlog.ViewModels.Home;

namespace CoolNetBlog.Bll
{

    /// <summary>
    /// 主页前台 基础业务处理类
    /// </summary>
    public class BaseLogicBll
    {
        public BaseLogicBll()
        {
        }

        /// <summary>
        /// 获取当前http请求上下文Host地址字符串：协议+域名+端口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>

        public static string GetCurrentHttpHostString(HttpContext context)
        {
            var scheme = context.Request.Scheme + "://";
            var currentHost = context.Request.Host.Host;
            var port = context.Request.Host.Port;
            var hostV = scheme + currentHost + (port == null ? "" : ":" + port);
            hostV = hostV.TrimEnd('/');
            return hostV;
        }

        public void DealSubMenu(List<HomeMenuViewModel> pMenus, List<HomeMenuViewModel> allMenus)
        {
            // 迭代顶级菜单 递归搜索下级菜单
            foreach (var pm in pMenus)
            {
                pm.Subs = allMenus.Where(m => m.PId == pm.Id).ToList();
                if (pm.Subs.Any())
                {
                    DealSubMenu(pm.Subs, allMenus);
                }
            }
        }

        /// <summary>
        /// 处理所需文章数据，主页分页 或具体菜单下的文章 或关键词搜索
        /// </summary>
        /// <param name="from"></param>
        /// <param name="menuId">若有，具体菜单下的文章 菜单Id</param>
        /// <param name="kw">若有，关键字搜索 关键字</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<int> DealFilterData(HomeViewModel homeGlobalView, BaseSugar bdb, string? from, int? menuId, string? kw, int pageIndex, int onePageCount)
        {

            var c = 0;
            if (from != null && from.ToLower().Trim() == "menu" && menuId != null)
            {
                // 点击了某菜单 菜单分页
                homeGlobalView.HomeArticleViewModels = await bdb._dbHandler.Queryable<HomeArticleViewModel>()
               .IgnoreColumns(a => a.Content)
               .Where(a => a.IsDraft == false && a.MenuId == menuId)
               .OrderBy(a => a.UpdateTime, SqlSugar.OrderByType.Desc)
               .Skip((pageIndex - 1) * onePageCount)
               .Take(onePageCount).ToListAsync();
                // 返回此条件下的总数量 供之后处理分页逻辑使用
                var mn = (await bdb._dbHandler.Queryable<Menu>().FirstAsync(m => m.Id == menuId)).Name;
                c = await bdb._dbHandler.Queryable<HomeArticleViewModel>().Where(a => a.IsDraft == false && a.MenuId == menuId).CountAsync();
                homeGlobalView.LocationTip = "菜单 " + mn;
                homeGlobalView.Location = "menu";
            }
            else if (from != null && from.ToLower().Trim() == "keyword" && kw != null && !String.IsNullOrWhiteSpace(kw.ToString()))
            {
                // 关键字搜索分页
                var keyword = kw?.ToString()?.Trim() ?? "";
                // 先在查询过滤的句柄停留，获取过滤条件后的总数
                var queryHandler = bdb._dbHandler.Queryable<HomeArticleViewModel>()
                .IgnoreColumns(a => a.Content)
                .Where(a => a.IsDraft == false)
                .Where(a =>
                        (a.Abstract != null && a.Abstract.Contains(keyword)) ||
                        (a.Title != null && a.Title.Contains(keyword)) ||
                        //(a.Content!=null&&a.Content.Contains(keyword))||
                        (a.Labels != null && a.Labels.Contains(keyword)));

                c = await queryHandler.CountAsync();

                // 再继续数据库句柄，获取当前分页数据
                homeGlobalView.HomeArticleViewModels = await queryHandler
                .OrderBy(a => a.UpdateTime, SqlSugar.OrderByType.Desc)
                .Skip((pageIndex - 1) * onePageCount)
                .Take(onePageCount).ToListAsync();
                // 返回此条件下的总数量 供之后处理分页逻辑使用
                homeGlobalView.LocationTip = $"有关“{keyword}”搜索后的内容";
                homeGlobalView.Location = "keyword";
            }
            else
            {
                // 主页分页
                homeGlobalView.HomeArticleViewModels = await bdb._dbHandler.Queryable<HomeArticleViewModel>()
                .IgnoreColumns(a => a.Content)
                .Where(a => a.IsDraft == false)
                .OrderBy(a => a.UpdateTime, SqlSugar.OrderByType.Desc)
                .Skip((pageIndex - 1) * onePageCount)
                .Take(onePageCount).ToListAsync();
                // 返回此条件下的总数量 供之后处理分页逻辑使用
                c = await bdb._dbHandler.Queryable<HomeArticleViewModel>().Where(a => a.IsDraft == false).CountAsync();
            }
            return c;
        }

        /// <summary>
        /// 处理分页逻辑
        /// </summary>
        /// <param name="c">当前按条件过滤后的总数</param>
        /// <param name="pageIndex"></param>
        /// <param name="onePageCount"></param>
        public void ComputePage(HomeViewModel homeGlobalView, int c, int pageIndex, int onePageCount)
        {
            //// 别忘了忽略条件再获取总数
            //var c = (await bdb._dbHandler.Queryable<HomeArticleViewModel>().Where(a => a.IsDraft == false).CountAsync());


            if (c > (pageIndex) * onePageCount)
            {
                // 总数 大于 当前分页*每页条数  显示下一页按钮
                homeGlobalView.PageCompute.ShowNextIndex = true;
                homeGlobalView.PageCompute.ShowPreIndex = true;
                if (c >= 0 && c <= onePageCount)
                {
                    homeGlobalView.PageCompute.ShowPreIndex = false;
                }
                if (pageIndex == 1)
                {
                    homeGlobalView.PageCompute.ShowPreIndex = false;
                }
                homeGlobalView.PageCompute.NextIndex = pageIndex + 1;
                homeGlobalView.PageCompute.PreIndex = pageIndex - 1 < 1 ? 1 : pageIndex - 1;
                homeGlobalView.PageCompute.PageIndex = pageIndex;
            }
            else
            {
                homeGlobalView.PageCompute.ShowNextIndex = false;
                homeGlobalView.PageCompute.ShowPreIndex = true;
                if (c >= 0 && c <= onePageCount)
                {
                    homeGlobalView.PageCompute.ShowPreIndex = false;

                }
                if (pageIndex == 1)
                {
                    homeGlobalView.PageCompute.ShowPreIndex = false;
                }
                homeGlobalView.PageCompute.PreIndex = pageIndex - 1 < 1 ? 1 : pageIndex - 1;
                homeGlobalView.PageCompute.NextIndex = 1;
                homeGlobalView.PageCompute.PageIndex = pageIndex;
            }
        }
    }
}
