import { parseInvite, inviteCode } from './invite.js';

const translations = {
  en: {title:['Show what','you mean.'],pageTitle:'Draw on your screen',description:'Mark up your screen, cover private details, or draw together on a shared canvas.',download:'Download NovaPen',details:'Native app. Free and open source.',invite:'You have a canvas invitation. In NovaPen, open Canvas → Connect (the chain icon) and paste the code.',copy:'Copy invite code',copied:'Copied',copyFailed:'Copy the invitation from the address bar.',language:'Language',downloads:'Downloads',downloadFor:'Download NovaPen for {platform}',license:'MIT License'},
  ru: {title:['Проще показать,','чем объяснить.'],pageTitle:'Рисуйте на экране',description:'Подчёркивайте важное, закрывайте личные данные масками и рисуйте вместе на общем полотне.',download:'Скачать NovaPen',details:'Нативное приложение. Бесплатно и с открытым кодом.',invite:'Вам прислали приглашение к полотну. В NovaPen откройте «Полотно → Подключить» (значок цепочки) и вставьте код.',copy:'Копировать код',copied:'Скопировано',copyFailed:'Скопируйте приглашение из адресной строки.',language:'Язык',downloads:'Скачать приложение',downloadFor:'Скачать NovaPen для {platform}',license:'Лицензия MIT'},
  'zh-Hans': {title:['画出来，','一看就懂。'],pageTitle:'在屏幕上自由绘画',description:'在屏幕上标注重点，遮住隐私内容，也能在共享画布上一起画。',download:'下载 NovaPen',details:'原生应用，免费开源。',invite:'你收到了画布邀请。在 NovaPen 中打开“画布 → 连接”（链条图标），然后粘贴邀请码。',copy:'复制邀请码',copied:'已复制',copyFailed:'请从地址栏复制邀请链接。',language:'语言',downloads:'下载应用',downloadFor:'下载适用于 {platform} 的 NovaPen',license:'MIT 许可证'},
  fr: {title:['Montrez ce que','vous voulez dire.'],pageTitle:'Dessinez sur votre écran',description:'Annotez votre écran, masquez les informations privées ou dessinez ensemble sur une toile partagée.',download:'Télécharger NovaPen',details:'Application native. Gratuite et open source.',invite:'Vous avez une invitation. Dans NovaPen, ouvrez Toile → Connecter (l’icône de chaîne) et collez le code.',copy:'Copier le code',copied:'Copié',copyFailed:'Copiez l’invitation depuis la barre d’adresse.',language:'Langue',downloads:'Téléchargements',downloadFor:'Télécharger NovaPen pour {platform}',license:'Licence MIT'},
  de: {title:['Zeigen Sie,','was Sie meinen.'],pageTitle:'Zeichnen Sie auf Ihrem Bildschirm',description:'Markieren Sie Inhalte auf dem Bildschirm, verdecken Sie private Daten oder zeichnen Sie gemeinsam.',download:'NovaPen herunterladen',details:'Native Anwendung. Kostenlos und quelloffen.',invite:'Sie haben eine Einladung. Öffnen Sie in NovaPen Zeichenfläche → Verbinden (das Kettensymbol) und fügen Sie den Code ein.',copy:'Code kopieren',copied:'Kopiert',copyFailed:'Kopieren Sie die Einladung aus der Adressleiste.',language:'Sprache',downloads:'Downloads',downloadFor:'NovaPen für {platform} herunterladen',license:'MIT-Lizenz'}
};
const languageKey='novapen.site.language';
const languageSelect=document.getElementById('language');
let text, manualLanguage=false, savedLanguage;
try { savedLanguage=localStorage.getItem(languageKey); } catch { /* Private browsing may disable storage. */ }
function browserLanguage() {
  for(const language of navigator.languages?.length?navigator.languages:[navigator.language||'en']) {
    const base=language.split('-')[0].toLowerCase();
    if(base==='zh') return 'zh-Hans';
    if(Object.hasOwn(translations,base)) return base;
  }
  return 'en';
}
function setLanguage(language) {
  if(!Object.hasOwn(translations,language)) language='en';
  text=translations[language];
  document.documentElement.lang=language;
  document.title=`NovaPen — ${text.pageTitle}`;
  document.querySelector('meta[name="description"]').content=text.description;
  languageSelect.value=language;
  languageSelect.setAttribute('aria-label',text.language);
  document.querySelector('.downloads').setAttribute('aria-label',text.downloads);
  for(const element of document.querySelectorAll('[data-text]')) {
    const value=text[element.dataset.text];
    if(Array.isArray(value)) element.replaceChildren(document.createTextNode(value[0]),document.createElement('br'),document.createTextNode(value[1]));
    else element.textContent=value;
  }
  for(const link of document.querySelectorAll('.download'))
    link.setAttribute('aria-label',text.downloadFor.replace('{platform}',link.querySelector('strong').textContent));
  invitation();
}
if(Object.hasOwn(translations,savedLanguage)) manualLanguage=true;
setLanguage(manualLanguage?savedLanguage:browserLanguage());
languageSelect.addEventListener('change',()=>{
  manualLanguage=true;
  setLanguage(languageSelect.value);
  try { localStorage.setItem(languageKey,languageSelect.value); } catch { /* Keep the choice for this visit. */ }
});
addEventListener('languagechange',()=>{if(!manualLanguage)setLanguage(browserLanguage());});

const platform=/Mac/i.test(navigator.platform)?'macos':/Linux/i.test(navigator.platform)?'linux':'windows';
const primary=document.getElementById(platform);
for(const link of document.querySelectorAll('.download')) link.classList.toggle('primary',link===primary);

// Public release metadata only. The page never reads canvas data or sends an invite key.
fetch('https://api.github.com/repos/Xekep/NovaPen/releases/latest', {credentials:'omit',referrerPolicy:'no-referrer',signal:AbortSignal.timeout(5000)})
  .then(response=>response.ok?response.json():Promise.reject())
  .then(release=>{
    const patterns={windows:/-win-x64-Setup\.exe$/,linux:/^novapen_.*_amd64\.deb$/,arch:/^novapen-[0-9].*-x86_64\.pkg\.tar\.zst$/,void:/^novapen-[0-9].*\.x86_64\.xbps$/,macos:/^NovaPen-osx-arm64\.tar\.gz$/};
    for(const [os,pattern] of Object.entries(patterns)) {
      const asset=release.assets?.find(asset=>pattern.test(asset.name));
      if(asset?.browser_download_url?.startsWith('https://github.com/Xekep/NovaPen/releases/download/')) document.getElementById(os).href=asset.browser_download_url;
    }
  }).catch(()=>{});

function invitation() {
  const section=document.getElementById('invitation');section.hidden=true;
  try {
    if(!location.hash)return;
    const code=inviteCode(parseInvite(location.href));section.hidden=false;
    const button=document.getElementById('copy-invite');button.textContent=text.copy;
    button.onclick=async()=>{
      try { await navigator.clipboard.writeText(code);button.textContent=text.copied; }
      catch { button.textContent=text.copyFailed; }
    };
  } catch { /* An unrelated fragment does not expose any invitation UI. */ }
}
addEventListener('hashchange',invitation);
