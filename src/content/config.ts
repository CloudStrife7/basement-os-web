import { defineCollection, z } from 'astro:content';

const devlog = defineCollection({
  schema: z.object({
    title: z.string(),
    date: z.coerce.date(),
    // tags: z.array(z.string()).optional(),
    type: z.enum(['milestone', 'update', 'meta', 'skip']).default('update'),
    description: z.string().optional(),
    layout: z.string().optional(),
  }),
});

export const collections = { devlog };
