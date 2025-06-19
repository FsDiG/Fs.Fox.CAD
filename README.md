# Fs.Fox 分支说明
## IFoxCAD

官方地址: [IFoxCAD: 基于.NET的Cad二次开发类库](https://gitee.com/inspirefunction/ifoxcad)
或参考本分支下的:  IFoxCAD 说明.md


## Fs.Fox 分支说明

在IFoxCAD的基础上,将命名空间改为Fs.Fox,并作为Fs团队AutoCAD的基础库之一使用;
重要更改会尽量回馈给IFox,不过IFox对PR处理速度太慢了, 我基本已经放弃~
### 在线导图
我基于公开文档草稿整理了思维导图, boardmix链接中的文件「.NET ARX_Fox」
https://boardmix.cn/app/share/CAE.CMvmgA4gASoQHBGpsUGmGR9LipooomyTSDAGQAE/U41nx2，
需要编辑权限的欢迎私聊我

## Why

以便Fs的团队在生产环境中使用,避免Dll版本的冲突
希望能够灵活和自由维护和开发此类库
IFox0.9版本正在重构，存在不稳定因素,原计划去掉.net48的支持,不排除后续有大的变动;
IFox PullRequest目前看反馈速度也比较慢

## 其他

### ToDo
- [ ] 评估第三方IndexRange包,是否要清理
C#原生IndexRange 是在Standard 2.1引入的,Framework 4.8 仅支持Standard 2.0;


### 怎么通过本仓库给Gitee上的Fox官方贡献代码
已放弃,下面不用看
~建议配置两个远程地址,gitee的可以跟fox仓库进行pr等关联操作~
~推荐从gitee-->github~
[remote "origin"]
	url = git@gitee.com:bim360/Fs.Fox.CAD.git
	url = git@github.com:FsDiG/Fs.Fox.CAD.git
