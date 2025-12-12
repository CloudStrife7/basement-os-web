
import fs from 'fs';
import path from 'path';

const dir = 'src/content/devlog';
const files = fs.readdirSync(dir).filter(f => f.endsWith('.md'));

files.forEach(file => {
    const filePath = path.join(dir, file);
    let content = fs.readFileSync(filePath, 'utf8');

    // Extract frontmatter
    const match = content.match(/^---\s*([\s\S]*?)\s*---([\s\S]*)$/);
    if (!match) {
        console.log(`Skipping ${file}: No frontmatter found`);
        return;
    }

    let [_, fm, body] = match;
    let newFm = fm;

    // Fix Date: Remove quotes if present
    newFm = newFm.replace(/date:\s*"?(\d{4}-\d{2}-\d{2})"?/g, 'date: $1');

    // Remove layout field
    newFm = newFm.replace(/layout:.*\n?/g, '');

    // Reassemble
    const newContent = `---\n${newFm}\n---\n${body.trim()}\n`;
    fs.writeFileSync(filePath, newContent);
    console.log(`Fixed ${file}`);
});
