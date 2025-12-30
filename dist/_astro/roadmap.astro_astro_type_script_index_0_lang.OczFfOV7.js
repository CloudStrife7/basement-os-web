function $(){if(window.__roadmapInitialized)return;window.__roadmapInitialized=!0;const M="/data/roadmap.json";function E(e){const s=e.map(a=>typeof a=="string"?a:a.name);return s.includes("priority: critical")?"critical":s.includes("priority: high")?"high":s.includes("priority: medium")?"medium":"low"}function T(e){if(typeof e=="object"&&e.color)return`#${e.color}`;const s=typeof e=="string"?e:e.name;return s==="bug"?"#ff4444":s==="enhancement"?"#44aa44":s==="documentation"?"#0066cc":s.startsWith("area:")?"#6666ff":s.startsWith("priority:")?"#ffaa00":s==="concept"?"#aa66cc":s==="maintenance"?"#888888":s==="needs-testing"?"#ff8800":"#666666"}function _(e){return e.filter(s=>!(typeof s=="string"?s:s.name).startsWith("priority:")).slice(0,4)}function I(e){return e.replace(/^\[.*?\]\s*/,"")}function C(e){return e.charAt(0).toUpperCase()+e.slice(1)}function k(e){const s=new Date(e),i=new Date().getTime()-s.getTime(),n=Math.floor(i/(1e3*60*60*24));return n===0?"Today":n===1?"Yesterday":n<7?`${n} days ago`:n<30?`${Math.floor(n/7)} weeks ago`:n<365?`${Math.floor(n/30)} months ago`:`${Math.floor(n/365)} years ago`}function y(e,s=!1){const a=e.labels||[],i=E(a),n=_(a),l=document.createElement("div");l.className=`issue-card ${s?"completed":`priority-${i}`}`;const h=n.length>0?n.map(o=>{const f=typeof o=="string"?o:o.name,d=T(o),t=D(d)?"#000":"#fff";return`<span class="issue-label" style="background: ${d}; color: ${t}">${f.replace("area: ","")}</span>`}).join(""):'<span style="color: var(--text-dim); font-style: italic;">None</span>',g=e.milestone?`<div class="issue-row">
                <span class="issue-row-label">Milestone</span>
                <span class="issue-row-value issue-milestone">${e.milestone}</span>
               </div>`:"",v=s?"✓":"●",p=s?"closed":"open",m=s?"Closed":"Open";return l.innerHTML=`
            <div class="issue-card-header">
                <span class="issue-status-icon ${p}">${v}</span>
                <span class="issue-title">${I(e.title)}</span>
                <span class="issue-number">#${e.number}</span>
            </div>
            <div class="issue-card-body">
                <div class="issue-row">
                    <span class="issue-row-label">Status</span>
                    <span class="issue-row-value">${m}</span>
                </div>
                <div class="issue-row">
                    <span class="issue-row-label">Priority</span>
                    <span class="issue-row-value priority-${i}">${C(i)}</span>
                </div>
                <div class="issue-row">
                    <span class="issue-row-label">Labels</span>
                    <span class="issue-row-value"><div class="issue-labels">${h}</div></span>
                </div>
                ${g}
                <div class="issue-row">
                    <span class="issue-row-label">Created</span>
                    <span class="issue-row-value">${k(e.created_at)}</span>
                </div>
            </div>
        `,l}function D(e){const s=e.replace("#",""),a=parseInt(s.substr(0,2),16),i=parseInt(s.substr(2,2),16),n=parseInt(s.substr(4,2),16);return(a*299+i*587+n*114)/1e3>155}function H(e){const s=e.open_issues+e.closed_issues>0?Math.round(e.closed_issues/(e.open_issues+e.closed_issues)*100):0,a=document.createElement("div");return a.className="milestone-card",a.innerHTML=`
            <div class="milestone-title">${e.title}</div>
            <div class="milestone-progress">
                <div class="milestone-progress-bar" style="width: ${s}%"></div>
            </div>
            <div class="milestone-stats">
                <span>${e.closed_issues} done</span>
                <span>${e.open_issues} remaining</span>
                <span>${s}%</span>
            </div>
        `,a}function N(e){const{issues:s,milestones:a,closed:i}=e,n=s.filter(t=>{const u=(t.labels||[]).map(c=>typeof c=="string"?c:c.name);return u.includes("priority: critical")||u.includes("priority: high")}),l=s.filter(t=>(t.labels||[]).map(c=>typeof c=="string"?c:c.name).includes("priority: medium")),h=s.filter(t=>{const r=t.labels||[],u=r.map(L=>typeof L=="string"?L:L.name);return E(r)==="low"||!u.includes("priority: critical")&&!u.includes("priority: high")&&!u.includes("priority: medium")}),g=new Date;g.setDate(g.getDate()-30);const v=i.filter(t=>t.closed_at&&new Date(t.closed_at)>g),p=document.getElementById("current-focus");p&&(p.innerHTML="",n.length===0?p.innerHTML='<div class="empty-state">No high-priority items right now</div>':n.forEach(t=>p.appendChild(y(t))));const m=document.getElementById("milestones");if(m){m.innerHTML="";const t=a.filter(r=>r.state==="open");t.length===0?m.innerHTML='<div class="empty-state">No active milestones</div>':t.forEach(r=>m.appendChild(H(r)))}const o=document.getElementById("coming-soon");o&&(o.innerHTML="",l.length===0?o.innerHTML='<div class="empty-state">No medium-priority items queued</div>':l.forEach(t=>o.appendChild(y(t))));const f=document.getElementById("backlog");f&&(f.innerHTML="",h.length===0?f.innerHTML='<div class="empty-state">Backlog is empty</div>':h.forEach(t=>f.appendChild(y(t))));const d=document.getElementById("completed");if(d&&(d.innerHTML="",v.length===0?d.innerHTML='<div class="empty-state">No issues closed in the last 30 days</div>':v.slice(0,10).forEach(t=>d.appendChild(y(t,!0)))),e.generated_at){const t=new Date(e.generated_at).toLocaleString();console.log(`Roadmap data last synced: ${t}`)}}const b=document.getElementById("toggle-backlog"),w=document.getElementById("backlog");b&&w&&b.addEventListener("click",()=>{w.classList.toggle("collapsed"),b.textContent=w.classList.contains("collapsed")?"Show All Backlog Items":"Collapse Backlog"}),fetch(M).then(e=>{if(!e.ok)throw new Error("Roadmap data not found");return e.json()}).then(e=>{N(e)}).catch(e=>{console.error("Failed to load roadmap:",e),document.querySelectorAll(".loading-indicator").forEach(s=>{s.textContent="Roadmap data not yet synced"})})}document.addEventListener("astro:before-swap",()=>{window.__roadmapInitialized=!1});document.addEventListener("astro:page-load",$);document.addEventListener("DOMContentLoaded",$);document.readyState!=="loading"&&$();
