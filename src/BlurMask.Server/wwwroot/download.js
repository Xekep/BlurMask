import { parseInvite, inviteCode } from './invite.js';

const translations = {
  en: {title:['Show what','you mean.'],description:'Mark up your screen, cover private details, or draw together on a shared canvas.',download:'Download BlurMask',details:'Native app. Free and open source.',invite:'You have a canvas invitation. In BlurMask, open Canvas → Connect (the chain icon) and paste the code.',copy:'Copy invitation',copied:'Copied',copyFailed:'Copy the invitation from the address bar.'},
  ru: {title:['Проще показать,','чем объяснить.'],description:'Подчёркивайте важное, закрывайте личные данные масками и рисуйте вместе на общем полотне.',download:'Скачать BlurMask',details:'Нативное приложение. Бесплатно и с открытым кодом.',invite:'Вам прислали приглашение к полотну. В BlurMask откройте «Полотно → Подключить» (значок цепочки) и вставьте код.',copy:'Копировать приглашение',copied:'Скопировано',copyFailed:'Скопируйте приглашение из адресной строки.'},
  fr: {title:['Montrez ce que','vous voulez dire.'],description:'Annotez votre écran, masquez les informations privées ou dessinez ensemble sur une toile partagée.',download:'Télécharger BlurMask',details:'Application native. Gratuite et open source.',invite:'Vous avez une invitation. Dans BlurMask, ouvrez Toile → Connecter (l’icône de chaîne) et collez le code.',copy:'Copier l’invitation',copied:'Copié',copyFailed:'Copiez l’invitation depuis la barre d’adresse.'},
  de: {title:['Zeigen Sie,','was Sie meinen.'],description:'Markieren Sie Inhalte auf dem Bildschirm, verdecken Sie private Daten oder zeichnen Sie gemeinsam.',download:'BlurMask herunterladen',details:'Native Anwendung. Kostenlos und quelloffen.',invite:'Sie haben eine Einladung. Öffnen Sie in BlurMask Zeichenfläche → Verbinden (das Kettensymbol) und fügen Sie den Code ein.',copy:'Einladung kopieren',copied:'Kopiert',copyFailed:'Kopieren Sie die Einladung aus der Adressleiste.'}
};
const language=(navigator.language||'en').split('-')[0].toLowerCase();
const text=translations[language]??translations.en;
document.documentElement.lang=translations[language]?language:'en';
for(const element of document.querySelectorAll('[data-text]')) {
  const value=text[element.dataset.text];
  if(Array.isArray(value)) element.replaceChildren(document.createTextNode(value[0]),document.createElement('br'),document.createTextNode(value[1]));
  else element.textContent=value;
}
const platform=/Mac/i.test(navigator.platform)?'macos':/Linux/i.test(navigator.platform)?'linux':'windows';
const primary=document.getElementById(platform);
for(const link of document.querySelectorAll('.download')) link.classList.toggle('primary',link===primary);

// Public release metadata only. The page never reads canvas data or sends an invite key.
fetch('https://api.github.com/repos/Xekep/BlurMask/releases/latest', {credentials:'omit',referrerPolicy:'no-referrer',signal:AbortSignal.timeout(5000)})
  .then(response=>response.ok?response.json():Promise.reject())
  .then(release=>{
    const patterns={windows:/-win-x64-Setup\.exe$/,linux:/^blurmask_.*_amd64\.deb$/,arch:/^blurmask-[0-9].*-x86_64\.pkg\.tar\.zst$/,void:/^blurmask-[0-9].*\.x86_64\.xbps$/,macos:/^BlurMask-osx-arm64\.tar\.gz$/};
    for(const [os,pattern] of Object.entries(patterns)) {
      const asset=release.assets?.find(asset=>pattern.test(asset.name));
      if(asset?.browser_download_url?.startsWith('https://github.com/Xekep/BlurMask/releases/download/')) document.getElementById(os).href=asset.browser_download_url;
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
invitation();addEventListener('hashchange',invitation);
