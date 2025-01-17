﻿using ComponentsServices.Base;
using CoolNetBlog.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CoolNetBlog.Base
{
    public class BaseAdminController:Controller
    {
        protected static BaseSugar _bdb;

        public static PassBaseViewModel spassVm = new();
        /// <summary>
        /// 当前cookie的值，登录后生成此随机值。后续后台的操作都会从客户端取cookie验证值是否是一开始登陆时设置的此值
        /// </summary>
        public static string? _currentCookieValue = null;

        public BaseAdminController()
        {
            if (_bdb is null)
                _bdb = new BaseSugar();
        }
        /// <summary>
        /// 任何基础视图模型类可以继承自PassBaseViewModel类，以封装数据。<br/>
        /// 一些公共属性数据存储于PassBaseViewModel父类中,必要时调用此方法以自动封装属性
        /// </summary>
        /// <param name="pvm"></param>
        /// <returns></returns>
        protected PassBaseViewModel WrapMustNeedPassFields(PassBaseViewModel pvm)
        {
            pvm.AccountName = spassVm.AccountName;
            pvm.PassToken = spassVm.PassToken;
            return pvm;
        }

    }
}
