import { useEffect, useMemo, useState } from 'react';
import { Anchor, BookOpen, Boxes, CircleDollarSign, ClipboardList, Code2, ExternalLink, ShieldCheck, Waves } from 'lucide-react';
import { ClaimsApi } from '../api/ClaimsApi';
import type { Cover } from '../api/contracts';
import { ClaimsPanel } from '../components/ClaimsPanel';
import { CoversPanel } from '../components/CoversPanel';
import { PremiumPanel } from '../components/PremiumPanel';

type Tab = 'claims'|'covers'|'premium';
const initialUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5180';
export default function App() {
  const [tab,setTab]=useState<Tab>('claims'), [baseUrl,setBaseUrl]=useState(initialUrl), [draftUrl,setDraftUrl]=useState(initialUrl), [covers,setCovers]=useState<Cover[]>([]);
  const api=useMemo(()=>new ClaimsApi(baseUrl),[baseUrl]);
  useEffect(() => { let active = true; api.listCovers().then(rows => { if (active) setCovers(rows); }).catch(() => { if (active) setCovers([]); }); return () => { active = false; }; }, [api]);
  const nav: {id:Tab;label:string;icon:typeof ClipboardList}[]=[{id:'claims',label:'Claims',icon:ClipboardList},{id:'covers',label:'Covers',icon:Anchor},{id:'premium',label:'Premium',icon:CircleDollarSign}];
  return <main className="app-shell"><aside className="sidebar"><div className="brand"><div className="brand-mark"><Waves size={22}/></div><div><strong>Harbor<span>line</span></strong><small>INSURANCE OPERATIONS</small></div></div><div className="intro"><span className="eyebrow">CLAIMS WORKSPACE</span><h1>Clear view.<br/><em>Confident cover.</em></h1><p>A compact console for insurance claims, policies and premium estimates.</p></div>
    <div className="info-block"><div className="info-title"><BookOpen size={16}/> APPLICATION</div><p>Claims Handling API backed by a layered .NET service. The interface uses the REST endpoints directly so you can create, retrieve, and remove records.</p><div className="layer-list"><span><b>01</b> Web API</span><span><b>02</b> Application · CQRS</span><span><b>03</b> Domain</span><span><b>04</b> Infrastructure · SQL Server</span></div></div>
    <div className="info-block rules"><div className="info-title"><ShieldCheck size={16}/> BUSINESS RULES</div><h3>Claims</h3><ul><li>Damage cost must be between 0 and 100,000.</li><li>Claim date must fall within its cover period.</li></ul><h3>Cover</h3><ul><li>Start date cannot be in the past.</li><li>Coverage period cannot exceed one year.</li></ul><h3>Premium</h3><ul><li>Base rate: 1,250 per day, adjusted by vessel type.</li><li>First 30 days at base rate; next 150 days receive a discount.</li><li>Remaining days receive the additional long term discount.</li></ul></div>
    <div className="info-block audit"><div className="info-title"><Boxes size={16}/> AUDITING</div><p>Audit writes are queued and processed in the background to keep API requests responsive.</p></div><div className="sidebar-foot">Claims Desk <span>·</span> API playground</div></aside>
    <section className="workspace"><header className="topbar"><div className="crumb"><span>WORKSPACE</span><b>/</b> Insurance claims</div><div className="api-url"><span className="status-dot"/> API endpoint <input aria-label="API base URL" value={draftUrl} onChange={e=>setDraftUrl(e.target.value)} onBlur={()=>draftUrl.trim()&&setBaseUrl(draftUrl.trim().replace(/\/$/,''))}/><a href={`${baseUrl}/swagger`} target="_blank" rel="noreferrer" aria-label="Open Swagger"><ExternalLink size={15}/></a></div></header>
      <div className="workspace-inner"><div className="welcome"><div><span className="eyebrow">OPERATIONS CONSOLE</span><h2>Good day. <span>What would you like to do?</span></h2></div><div className="welcome-icon"><Code2 size={20}/></div></div><div className="tabs" role="tablist" aria-label="API resources">{nav.map(({id,label,icon:Icon})=><button role="tab" aria-selected={tab===id} className={tab===id?'active':''} key={id} onClick={()=>setTab(id)}><Icon size={17}/>{label}{id==='covers'&&covers.length>0&&<span className="tab-count">{covers.length}</span>}</button>)}</div>
        <div role="tabpanel" className="tab-panel">{tab==='claims'&&<ClaimsPanel api={api} covers={covers}/ >}{tab==='covers'&&<CoversPanel api={api} onChange={setCovers}/ >}{tab==='premium'&&<PremiumPanel api={api}/>}</div><footer className="workspace-footer"><span>Claims Handling</span><span>Connect to <a href={`${baseUrl}/swagger`} target="_blank" rel="noreferrer">API documentation <ExternalLink size={12}/></a></span></footer></div>
    </section></main>;
}
