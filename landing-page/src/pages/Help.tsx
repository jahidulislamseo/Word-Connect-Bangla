
import { motion } from 'framer-motion';
import { HelpCircle, Search } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Help() {
  const faqs = [
    { q: "How do I earn more coins?", a: "You can earn coins by completing levels, finding bonus words, claiming daily rewards, or watching reward ads." },
    { q: "Is the game free to play?", a: "Yes! WordConnect Bangla is 100% free to play. You can unlock everything just by playing." },
    { q: "Can I play offline?", a: "Yes, the core game works offline. However, daily rewards and ads require an internet connection." },
    { q: "How do I use hints?", a: "Tap the 'HINT' button at the bottom left of the game screen. It costs 50 coins per hint." }
  ];

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 py-32 px-6">
      <div className="max-w-3xl mx-auto">
        <Link to="/" className="text-cyan-400 hover:text-cyan-300 font-bold mb-8 inline-block">← Back to Home</Link>
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="glass p-10 rounded-[40px] border-white/10"
        >
          <div className="w-16 h-16 bg-blue-500/10 rounded-2xl flex items-center justify-center mb-8">
            <HelpCircle className="w-8 h-8 text-blue-400" />
          </div>
          <h1 className="text-4xl font-black mb-6">Help Center</h1>
          
          <div className="relative mb-10">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 w-5 h-5" />
            <input 
              type="text" 
              placeholder="Search for help..." 
              className="w-full bg-white/5 border border-white/10 rounded-2xl py-4 pl-12 pr-6 focus:outline-none focus:border-cyan-500/50 transition-all"
            />
          </div>

          <div className="space-y-6">
            {faqs.map((faq, i) => (
              <div key={i} className="p-6 bg-white/5 rounded-3xl border border-white/5">
                <h3 className="text-lg font-bold mb-2 text-white">{faq.q}</h3>
                <p className="text-slate-400 leading-relaxed">{faq.a}</p>
              </div>
            ))}
          </div>
        </motion.div>
      </div>
    </div>
  );
}
