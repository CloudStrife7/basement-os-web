function w(){if(window.__roadmapInitialized)return;window.__roadmapInitialized=!0;const M="/data/roadmap.json";function E(e){const n=e.map(a=>typeof a=="string"?a:a.name);return n.includes("priority: critical")?"critical":n.includes("priority: high")?"high":n.includes("priority: medium")?"medium":"low"}function L(e){return e.filter(n=>!(typeof n=="string"?n:n.name).startsWith("priority:")).slice(0,4)}function $(e){return e.replace(/^\[.*?\]\s*/,"")}function _(e){return e.charAt(0).toUpperCase()+e.slice(1)}function T(e){const n=new Date(e),r=new Date().getTime()-n.getTime(),s=Math.floor(r/(1e3*60*60*24));return s===0?"Today":s===1?"Yesterday":s<7?`${s} days ago`:s<30?`${Math.floor(s/7)} weeks ago`:s<365?`${Math.floor(s/30)} months ago`:`${Math.floor(s/365)} years ago`}function m(e,n=!1){const a=e.labels||[],r=E(a),s=L(a),d=document.createElement("div");d.className=`info-card issue-card ${n?"completed":`priority-${r}`}`,s.length>0&&s.map(l=>`<span class="issue-label">[ ${(typeof l=="string"?l:l.name).replace("area: ","").toUpperCase()} ]</span>`).join(" ");const u=e.milestone?`<div class="stat-row milestone-row">
                <span class="stat-label">Milestone</span>
                <span class="stat-value">${e.milestone}</span>
               </div>`:"";return d.innerHTML=`
            <h3>[ ${$(e.title)} #${e.number} ]</h3>
            <div class="stat-row">
                <span class="stat-label">Priority</span>
                <span class="stat-value priority-${r}">${_(r)}</span>
            </div>
            ${u}
            <div class="stat-row created-row">
                <span class="stat-label">Created</span>
                <span class="stat-value">${T(e.created_at)}</span>
            </div>
        `,d}function N(e){const n=e.open_issues+e.closed_issues>0?Math.round(e.closed_issues/(e.open_issues+e.closed_issues)*100):0,a=document.createElement("div");return a.className="milestone-card",a.innerHTML=`
            <div class="milestone-title">${e.title}</div>
            <div class="milestone-progress">
                <div class="milestone-progress-bar" style="width: ${n}%"></div>
            </div>
            <div class="milestone-stats">
                <span>${e.closed_issues} done</span>
                <span>${e.open_issues} remaining</span>
                <span>${n}%</span>
            </div>
        `,a}function D(e){const{issues:n,milestones:a,closed:r}=e,s=n.filter(t=>{const c=(t.labels||[]).map(o=>typeof o=="string"?o:o.name);return c.includes("priority: critical")||c.includes("priority: high")}),d=n.filter(t=>(t.labels||[]).map(o=>typeof o=="string"?o:o.name).includes("priority: medium")),u=n.filter(t=>{const i=t.labels||[],c=i.map(b=>typeof b=="string"?b:b.name);return E(i)==="low"||!c.includes("priority: critical")&&!c.includes("priority: high")&&!c.includes("priority: medium")}),l=new Date;l.setDate(l.getDate()-30);const v=r.filter(t=>t.closed_at&&new Date(t.closed_at)>l),p=document.getElementById("current-focus");p&&(p.innerHTML="",s.length===0?p.innerHTML='<div class="empty-state">No high-priority items right now</div>':s.forEach(t=>p.appendChild(m(t))));const f=document.getElementById("milestones");if(f){f.innerHTML="";const t=a.filter(i=>i.state==="open");t.length===0?f.innerHTML='<div class="empty-state">No active milestones</div>':t.forEach(i=>f.appendChild(N(i)))}const y=document.getElementById("coming-soon");y&&(y.innerHTML="",d.length===0?y.innerHTML='<div class="empty-state">No medium-priority items queued</div>':d.forEach(t=>y.appendChild(m(t))));const g=document.getElementById("backlog");g&&(g.innerHTML="",u.length===0?g.innerHTML='<div class="empty-state">Backlog is empty</div>':u.forEach(t=>g.appendChild(m(t))));const h=document.getElementById("completed");if(h&&(h.innerHTML="",v.length===0?h.innerHTML='<div class="empty-state">No issues closed in the last 30 days</div>':v.slice(0,10).forEach(t=>h.appendChild(m(t,!0)))),e.generated_at){const t=new Date(e.generated_at).toLocaleString();console.log(`Roadmap data last synced: ${t}`)}}fetch(M).then(e=>{if(!e.ok)throw new Error("Roadmap data not found");return e.json()}).then(e=>{D(e)}).catch(e=>{console.error("Failed to load roadmap:",e),document.querySelectorAll(".loading-indicator").forEach(n=>{n.textContent="Roadmap data not yet synced"})})}document.addEventListener("astro:before-swap",()=>{window.__roadmapInitialized=!1});document.addEventListener("astro:page-load",w);document.addEventListener("DOMContentLoaded",w);document.readyState!=="loading"&&w();
