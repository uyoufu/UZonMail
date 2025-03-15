import{_ as i}from"./plugin-vue_export-helper-DlAUqK2U.js";import{c as a,a as n,o as e}from"./app-CJsgKil_.js";const t={};function l(p,s){return e(),a("div",null,s[0]||(s[0]=[n(`<p>本文介绍企业版本相关的配置</p><h2 id="专属配置" tabindex="-1"><a class="header-anchor" href="#专属配置"><span>专属配置</span></a></h2><h3 id="取消订阅" tabindex="-1"><a class="header-anchor" href="#取消订阅"><span>取消订阅</span></a></h3><p>在 <code>appsettings.Production.json</code> 中添加 <code>Unsubscribe</code> 字段可以针对不同的收件域名，配置不同的退订头。默认配置如下：</p><div class="language-json line-numbers-mode" data-highlighter="shiki" data-ext="json" data-title="json" style="--shiki-light:#383A42;--shiki-dark:#abb2bf;--shiki-light-bg:#FAFAFA;--shiki-dark-bg:#282c34;"><pre class="shiki shiki-themes one-light one-dark-pro vp-code"><code><span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">{</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">  &quot;Unsubscribe&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: {</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    &quot;Headers&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: [</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      {</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">        &quot;Domain&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;gmail.com&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">,</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">        &quot;Header&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;RFC8058&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">,</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">        &quot;Description&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;这个是默认的退订头&quot;</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      },</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      {</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">        &quot;Domain&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;aliyun.com&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">,</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">        &quot;Header&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;AliDM&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">,</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">        &quot;Description&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;阿里云的退订头&quot;</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      }</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">    ]</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">  },</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">}</span></span></code></pre><div class="line-numbers" aria-hidden="true" style="counter-reset:line-number 0;"><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div></div></div><ul><li>Domain 表示收件箱域名</li><li>Header 表示使用的头协议，目前程序实现了两种 <ul><li>RFC8058</li><li>AliDM</li></ul></li></ul>`,6)]))}const d=i(t,[["render",l],["__file","pro.html.vue"]]),o=JSON.parse('{"path":"/guide/setup/pro.html","title":"企业版","lang":"zh-CN","frontmatter":{"title":"企业版","icon":"circle-up","order":10,"description":"本文介绍企业版本相关的配置 专属配置 取消订阅 在 appsettings.Production.json 中添加 Unsubscribe 字段可以针对不同的收件域名，配置不同的退订头。默认配置如下： Domain 表示收件箱域名 Header 表示使用的头协议，目前程序实现了两种 RFC8058 AliDM","head":[["meta",{"property":"og:url","content":"https://uzonmail.pages.dev/guide/setup/pro.html"}],["meta",{"property":"og:site_name","content":"宇正群邮"}],["meta",{"property":"og:title","content":"企业版"}],["meta",{"property":"og:description","content":"本文介绍企业版本相关的配置 专属配置 取消订阅 在 appsettings.Production.json 中添加 Unsubscribe 字段可以针对不同的收件域名，配置不同的退订头。默认配置如下： Domain 表示收件箱域名 Header 表示使用的头协议，目前程序实现了两种 RFC8058 AliDM"}],["meta",{"property":"og:type","content":"article"}],["meta",{"property":"og:locale","content":"zh-CN"}],["meta",{"property":"og:updated_time","content":"2025-03-15T14:43:09.000Z"}],["meta",{"property":"article:modified_time","content":"2025-03-15T14:43:09.000Z"}],["script",{"type":"application/ld+json"},"{\\"@context\\":\\"https://schema.org\\",\\"@type\\":\\"Article\\",\\"headline\\":\\"企业版\\",\\"image\\":[\\"\\"],\\"dateModified\\":\\"2025-03-15T14:43:09.000Z\\",\\"author\\":[{\\"@type\\":\\"Person\\",\\"name\\":\\"galens\\",\\"url\\":\\"galens.uamazing.cn\\"}]}"]]},"headers":[{"level":2,"title":"专属配置","slug":"专属配置","link":"#专属配置","children":[{"level":3,"title":"取消订阅","slug":"取消订阅","link":"#取消订阅","children":[]}]}],"git":{"createdTime":1742049789000,"updatedTime":1742049789000,"contributors":[{"name":"galens","username":"galens","email":"gmx_galens@163.com","commits":1,"url":"https://github.com/galens"}]},"readingTime":{"minutes":0.4,"words":120},"filePathRelative":"guide/setup/pro.md","localizedDate":"2025年3月15日","autoDesc":true}');export{d as comp,o as data};
