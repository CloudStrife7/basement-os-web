
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const devlogDir = path.join(__dirname, 'src', 'content', 'devlog');

const files = fs.readdirSync(devlogDir);

files.forEach(file => {
    if (!file.endsWith('.md')) return;

    const filePath = path.join(devlogDir, file);
    let content = fs.readFileSync(filePath, 'utf-8');

    // Parse frontmatter
    const frontmatterRegex = /^---\s*([\s\S]*?)\s*---/;
    const match = content.match(frontmatterRegex);

    if (match) {
        let frontmatter = match[1];
        let body = content.replace(frontmatterRegex, '');

        let shouldBeMilestone = false;

        // Check tags for MILESTONE
        if (frontmatter.includes('tags:')) {
            const tagsMatch = frontmatter.match(/tags:\s*\[(.*?)\]/);
            if (tagsMatch && tagsMatch[1].toUpperCase().includes('"MILESTONE"')) {
                shouldBeMilestone = true;
            }
        }

        // Update type if needed
        if (shouldBeMilestone) {
            if (!frontmatter.includes('type: milestone')) {
                // If type exists, replace it, otherwise add it
                if (frontmatter.includes('type:')) {
                    frontmatter = frontmatter.replace(/type:\s*.*(\r?\n)/, 'type: milestone$1');
                } else {
                    frontmatter += '\ntype: milestone';
                }
                console.log(`Updated type to milestone for ${file}`);
            }
        }

        // Clean Title
        if (shouldBeMilestone || frontmatter.includes('type: milestone')) {
            const titleMatch = frontmatter.match(/title:\s*"(.*)"/);
            if (titleMatch) {
                let originalTitle = titleMatch[1];
                let newTitle = originalTitle
                    .replace(/^MILESTONE:\s*/i, '')
                    .replace(/^Critical Milestone:\s*/i, '')
                    .replace(/^Milestone:\s*/i, '');

                if (originalTitle !== newTitle) {
                    frontmatter = frontmatter.replace(`title: "${originalTitle}"`, `title: "${newTitle}"`);
                    console.log(`Cleaned title for ${file}: "${originalTitle}" -> "${newTitle}"`);
                }
            }
        }

        // Reconstruct file
        const newContent = `---${frontmatter}\n---${body}`;
        fs.writeFileSync(filePath, newContent, 'utf-8');
    }
});
