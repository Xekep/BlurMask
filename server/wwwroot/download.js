import { parseInvite, inviteCode } from './invite.js';

const translations = {
  en: {title:['Ideas over','any window.'],description:'Draw on your screen, hide distractions, and work together on a canvas.',windows:'Download for Windows',linux:'Linux .deb',details:'Native app. Free and open source.',footer:'A little chalk. A lot of ideas.',invite:'You have a canvas invitation. In BlurMask, open Canvas → Menu → Connect and paste the code.',copy:'Copy invitation',copied:'Copied',copyFailed:'Copy the invitation from the address bar.'},
  ru: {title:['Идеи поверх','любого окна.'],description:'Рисуйте на экране, скрывайте лишнее и работайте вместе на полотне.',windows:'Скачать для Windows',linux:'Linux .deb',details:'Нативное приложение. Бесплатно и с открытым кодом.',footer:'Немного мела. Много идей.',invite:'Вам прислали приглашение к полотну. В BlurMask откройте «Полотно → Меню → Подключить» и вставьте код.',copy:'Копировать приглашение',copied:'Скопировано',copyFailed:'Скопируйте приглашение из адресной строки.'},
  fr: {title:['Vos idées sur','chaque fenêtre.'],description:'Dessinez sur votre écran, masquez le superflu et collaborez sur une toile.',windows:'Télécharger pour Windows',linux:'Linux .deb',details:'Application native. Gratuite et open source.',footer:'Un peu de craie. Beaucoup d’idées.',invite:'Vous avez une invitation. Dans BlurMask, ouvrez Toile → Menu → Connecter et collez le code.',copy:'Copier l’invitation',copied:'Copié',copyFailed:'Copiez l’invitation depuis la barre d’adresse.'},
  de: {title:['Ideen auf','jedem Fenster.'],description:'Zeichnen Sie auf Ihrem Bildschirm, blenden Sie Inhalte aus und arbeiten Sie gemeinsam auf einer Zeichenfläche.',windows:'Für Windows herunterladen',linux:'Linux .deb',details:'Native Anwendung. Kostenlos und quelloffen.',footer:'Ein wenig Kreide. Viele Ideen.',invite:'Sie haben eine Einladung. Öffnen Sie in BlurMask Zeichenfläche → Menü → Verbinden und fügen Sie den Code ein.',copy:'Einladung kopieren',copied:'Kopiert',copyFailed:'Kopieren Sie die Einladung aus der Adressleiste.'}
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
primary.parentElement.prepend(primary);

// Public release metadata only. The page never reads canvas data or sends an invite key.
fetch('https://api.github.com/repos/Xekep/BlurMask/releases/latest', {credentials:'omit',referrerPolicy:'no-referrer',signal:AbortSignal.timeout(5000)})
  .then(response=>response.ok?response.json():Promise.reject())
  .then(release=>{
    const patterns={windows:/-win-x64-Setup\.exe$/,linux:/^blurmask_.*_amd64\.deb$/,macos:/^BlurMask-osx-arm64\.tar\.gz$/};
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
