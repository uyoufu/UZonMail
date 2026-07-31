# 优化邮箱格式验证器逻辑

1. 优化 D:\Develop\Personal\UzonMail\src\web\src\utils\validator.ts 、D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Database\Validators\InboxValidator.cs、D:\Develop\Personal\UzonMail\src\api\UZonMailUtils\Validators\StringValidator.cs, 分析邮箱验证逻辑是否存在可优化的地方，若有进行优化

2. D:\Develop\Personal\UzonMail\src\web\src\pages\emailManager\inbox\headerFunctions.ts , D:\Develop\Personal\UzonMail\src\web\src\pages\emailManager\inbox\useInboxImporter.ts 在导入邮箱之前，要对数据进行格式化，移除空格等肉眼无法分辨的符号