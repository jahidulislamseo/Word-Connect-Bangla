import { motion } from 'framer-motion';
import { 
  Download, 
  Smartphone, 
  Globe, 
  ShieldCheck, 
  Zap, 
  Trophy, 
  Star, 
  Play,
  Brain,
  Camera,
  Send,
  Share2,
  Volume2
} from 'lucide-react';
import { Link } from 'react-router-dom';
import { useState } from 'react';


export default function App() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 selection:bg-cyan-500/30">
      {/* Navbar */}
      <nav className="fixed top-0 w-full z-50 glass border-b border-white/5 py-4">
        <div className="max-w-7xl mx-auto px-6 flex justify-between items-center">
          <div className="flex items-center gap-2">
            <div className="w-10 h-10 bg-gradient-to-br from-cyan-500 to-purple-600 rounded-xl flex items-center justify-center font-black text-white shadow-lg">
              W
            </div>
            <span className="text-xl font-black tracking-tighter uppercase">WordConnect <span className="text-cyan-400">Bangla</span></span>
          </div>
          <div className="hidden md:flex items-center gap-8 text-sm font-bold text-slate-400">
            <a href="#features" className="hover:text-white transition-colors">Features</a>
            <a href="#how-to-play" className="hover:text-white transition-colors">How to Play</a>
            <Link to="/help" className="hover:text-white transition-colors">Help</Link>
            <a 
              href={import.meta.env.VITE_GAME_URL || "#"} 
              target="_blank"
              rel="noopener noreferrer"
              className="px-6 py-2 bg-cyan-500 text-slate-950 rounded-full hover:bg-cyan-400 transition-all font-black text-center"
            >
              PLAY WEB
            </a>


          </div>
        </div>
      </nav>

      {/* Hero Section */}
      <section className="relative pt-32 pb-20 px-6 overflow-hidden">
        <div className="max-w-7xl mx-auto grid md:grid-cols-2 gap-12 items-center">
          <motion.div
            initial={{ opacity: 0, x: -50 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.8 }}
          >
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-cyan-500/10 border border-cyan-500/20 text-cyan-400 text-xs font-black uppercase tracking-widest mb-6">
              <Zap className="w-3 h-3 fill-cyan-400" />
              Top Rated Word Game in Bangladesh
            </div>
            <h1 className="text-6xl md:text-8xl font-black leading-[0.9] mb-8 tracking-tighter">
              MASTERY IN <br />
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-cyan-400 via-blue-500 to-purple-600">
                EVERY SWIPE.
              </span>
            </h1>
            <p className="text-xl text-slate-400 mb-10 max-w-lg leading-relaxed">
              Experience the ultimate WordConnect game tailored for Bengali speakers. 
              Learn 1,000+ words, sharpen your brain, and enjoy stunning premium visuals.
            </p>
            <div className="flex flex-wrap gap-4">
              <button className="flex items-center gap-3 px-8 py-4 bg-white text-slate-950 rounded-2xl font-black hover:scale-105 transition-all shadow-xl">
                <Play className="w-5 h-5 fill-slate-950" />
                GET ON PLAY STORE
              </button>
              <button className="flex items-center gap-3 px-8 py-4 glass rounded-2xl font-black hover:bg-white/10 transition-all">
                <Smartphone className="w-5 h-5" />
                APP STORE
              </button>
            </div>
            
            <div className="mt-12 flex items-center gap-6">
              <div className="flex -space-x-4">
                {[1,2,3,4].map(i => (
                  <div key={i} className="w-12 h-12 rounded-full border-4 border-slate-950 bg-slate-800" />
                ))}
              </div>
              <div>
                <div className="flex text-yellow-400 mb-1">
                  {[1,2,3,4,5].map(i => <Star key={i} className="w-4 h-4 fill-current" />)}
                </div>
                <p className="text-sm font-bold text-slate-400">10k+ Players already joined</p>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, scale: 0.8, rotate: 10 }}
            animate={{ opacity: 1, scale: 1, rotate: 0 }}
            transition={{ duration: 1, type: "spring" }}
            className="relative"
          >
            <div className="absolute inset-0 bg-gradient-to-r from-cyan-500 to-purple-600 blur-[120px] opacity-20" />
            <img 
              src="/images/hero_mockup.png" 
              alt="WordConnect Bangla Phone Mockup" 
              className="relative z-10 w-full max-w-lg mx-auto drop-shadow-[0_50px_100px_rgba(0,0,0,0.5)] rounded-[3rem]"
            />
          </motion.div>

        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-24 px-6 bg-slate-900/50">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-4xl font-black mb-4 tracking-tight">WHY CHOOSE <span className="text-cyan-400">WORDCONNECT?</span></h2>
            <p className="text-slate-400 max-w-2xl mx-auto">Designed for learning, built for fun. The most complete vocabulary experience ever.</p>
          </div>
          
          <div className="grid md:grid-cols-3 gap-8">
            {[
              { 
                icon: Brain, 
                title: "1,000+ Words", 
                desc: "From basic daily words to advanced scientific terms, categorized for smooth learning." 
              },
              { 
                icon: Zap, 
                title: "Dynamic Visuals", 
                desc: "Backgrounds change based on the word category, providing visual context and stunning vibes." 
              },
              { 
                icon: Volume2, 
                title: "Audio Pronunciation", 
                desc: "Hear the correct English pronunciation of every word you find instantly." 
              },
              { 
                icon: Trophy, 
                title: "Rewards & Combos", 
                desc: "Find bonus words, earn coins, and maintain your combo streak for massive scores." 
              },
              { 
                icon: Globe, 
                title: "Bengali Native", 
                desc: "Fully localized with meanings in Bengali for a deep learning experience." 
              },
              { 
                icon: ShieldCheck, 
                title: "Safe & Educational", 
                desc: "Perfect for kids and adults. No time pressure, just pure brain training." 
              }
            ].map((f, i) => (
              <motion.div 
                key={i}
                whileHover={{ y: -10 }}
                className="p-8 glass rounded-3xl border-white/5 hover:border-cyan-500/30 transition-all"
              >
                <div className="w-14 h-14 bg-cyan-500/10 rounded-2xl flex items-center justify-center mb-6">
                  <f.icon className="w-7 h-7 text-cyan-400" />
                </div>
                <h3 className="text-2xl font-black mb-3 tracking-tight">{f.title}</h3>
                <p className="text-slate-400 leading-relaxed">{f.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* How to Play Section */}
      <section id="how-to-play" className="py-24 px-6">
        <div className="max-w-7xl mx-auto grid md:grid-cols-2 gap-16 items-center">
          <div>
            <h2 className="text-5xl font-black mb-8 leading-tight tracking-tighter">
              HOW TO <span className="text-cyan-400">PLAY?</span> <br />
              SIMPLE AS A SWIPE.
            </h2>
            <div className="space-y-8">
              {[
                { step: "01", title: "Observe the Circle", desc: "Look at the scrambled letters in the hexagonal ring at the bottom." },
                { step: "02", title: "Swipe to Connect", desc: "Drag your finger (or mouse) from one letter to another to form a valid word." },
                { step: "03", title: "Reveal the Meaning", desc: "Found a word? Watch it fly into the slots and hear its correct pronunciation." },
                { step: "04", title: "Find Bonus Words", desc: "Find hidden words not on the board to earn extra coins and rewards!" }
              ].map((s, i) => (
                <div key={i} className="flex gap-6">
                  <span className="text-3xl font-black text-cyan-500/20">{s.step}</span>
                  <div>
                    <h4 className="text-xl font-bold mb-2">{s.title}</h4>
                    <p className="text-slate-400">{s.desc}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
          <div className="glass rounded-[40px] p-4 border-white/10 relative overflow-hidden group">
            <div className="absolute inset-0 bg-gradient-to-br from-cyan-500/20 to-purple-600/20 opacity-0 group-hover:opacity-100 transition-opacity" />
            <img 
              src="/images/game_preview.png" 
              alt="Game Scene Preview" 
              className="w-full rounded-[30px] aspect-video object-cover transition-transform duration-700 group-hover:scale-110"
            />
            <div className="absolute inset-0 flex items-center justify-center">
              <a 
                href={import.meta.env.VITE_GAME_URL || "#"} 
                target="_blank"
                rel="noopener noreferrer"
                className="w-20 h-20 bg-white text-slate-950 rounded-full flex items-center justify-center shadow-2xl scale-0 group-hover:scale-100 transition-transform duration-500"
              >
                <Play className="w-8 h-8 fill-current translate-x-1" />
              </a>

            </div>
          </div>
        </div>
      </section>

      {/* Category Showcase */}
      <section className="py-24 px-6 bg-slate-950">
        <div className="max-w-7xl mx-auto">
          <div className="flex flex-col md:flex-row justify-between items-end gap-8 mb-16">
            <div className="max-w-2xl">
              <h2 className="text-5xl font-black mb-6 tracking-tighter uppercase">Dynamic <span className="text-cyan-400">Environments</span></h2>
              <p className="text-xl text-slate-400">The game adapts to the category of the word you're solving. Immerse yourself in different worlds as you learn.</p>
            </div>
          </div>
          
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {[
              { name: 'Nature', img: '/images/nature_bg.png', color: 'from-green-500' },
              { name: 'Animals', img: '/images/animal_bg.png', color: 'from-orange-500' },
              { name: 'Abstract', img: '/images/abstract_bg.png', color: 'from-purple-500' },
              { name: 'Places', img: '/images/place_bg.png', color: 'from-blue-500' }
            ].map((cat, i) => (
              <motion.div 
                key={i}
                whileHover={{ y: -10 }}
                className="relative aspect-[3/4] rounded-3xl overflow-hidden group cursor-pointer"
              >
                <img src={cat.img} alt={cat.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" />
                <div className={`absolute inset-0 bg-gradient-to-t ${cat.color} to-transparent opacity-40 group-hover:opacity-60 transition-opacity`} />
                <div className="absolute bottom-6 left-6">
                  <p className="text-2xl font-black uppercase tracking-widest">{cat.name}</p>
                </div>
              </motion.div>
            ))}
          </div>
        </div>
      </section>


      {/* Stats/Details Section */}
      <section className="py-20 px-6 bg-cyan-600">
        <div className="max-w-7xl mx-auto grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
          <div>
            <p className="text-5xl font-black text-slate-950 mb-2">1.1K+</p>
            <p className="text-cyan-100 font-bold uppercase tracking-widest text-xs">Words Included</p>
          </div>
          <div>
            <p className="text-5xl font-black text-slate-950 mb-2">10K+</p>
            <p className="text-cyan-100 font-bold uppercase tracking-widest text-xs">Active Players</p>
          </div>
          <div>
            <p className="text-5xl font-black text-slate-950 mb-2">4.9</p>
            <p className="text-cyan-100 font-bold uppercase tracking-widest text-xs">User Rating</p>
          </div>
          <div>
            <p className="text-5xl font-black text-slate-950 mb-2">100%</p>
            <p className="text-cyan-100 font-bold uppercase tracking-widest text-xs">Free to Play</p>
          </div>
        </div>
      </section>

      {/* Download Section */}
      <section id="download" className="py-24 px-6 relative overflow-hidden">
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[800px] bg-cyan-500/10 rounded-full blur-[150px]" />
        <div className="max-w-4xl mx-auto text-center relative z-10">
          <h2 className="text-5xl md:text-7xl font-black mb-8 tracking-tighter">READY TO START YOUR <br /> WORD JOURNEY?</h2>
          <p className="text-xl text-slate-400 mb-12 max-w-2xl mx-auto leading-relaxed">
            Download WordConnect Bangla today and join thousands of players sharpening their vocabulary every day.
          </p>
          <div className="flex flex-col md:flex-row justify-center gap-6">
            <button className="flex items-center justify-center gap-4 px-10 py-5 bg-white text-slate-950 rounded-[2rem] font-black text-xl hover:scale-105 transition-all shadow-[0_20px_40px_rgba(255,255,255,0.1)]">
              <Download className="w-6 h-6" />
              PLAY STORE
            </button>
            <button className="flex items-center justify-center gap-4 px-10 py-5 glass rounded-[2rem] font-black text-xl hover:bg-white/10 transition-all">
              <Smartphone className="w-6 h-6" />
              APP STORE
            </button>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="py-24 px-6 bg-slate-900/30">
        <div className="max-w-3xl mx-auto">
          <h2 className="text-4xl font-black mb-12 text-center uppercase tracking-widest">Frequently Asked <span className="text-cyan-400">Questions</span></h2>
          <div className="space-y-4">
            <FAQItem 
              question="Is WordConnect Bangla free?" 
              answer="Yes! The game is 100% free to play. We offer optional in-app purchases for hints and removing ads, but you can experience the full game without spending a penny."
            />
            <FAQItem 
              question="Can I play offline?" 
              answer="Absolutely. Once downloaded, you can play the classic mode anytime, anywhere without an internet connection."
            />
            <FAQItem 
              question="Is it suitable for children?" 
              answer="Yes, the game is designed for all ages. It's a great way for kids to build their English and Bengali vocabulary in a fun, safe environment."
            />
            <FAQItem 
              question="How many levels are there?" 
              answer="We currently have over 1,000 unique levels, with new words and challenges added every month!"
            />
          </div>
        </div>
      </section>


      {/* Footer */}
      <footer className="py-20 px-6 border-t border-white/5">
        <div className="max-w-7xl mx-auto grid md:grid-cols-4 gap-12 mb-16">
          <div className="col-span-2">
            <div className="flex items-center gap-2 mb-6">
              <div className="w-10 h-10 bg-cyan-500 rounded-xl flex items-center justify-center font-black text-slate-950">W</div>
              <span className="text-xl font-black uppercase tracking-tighter">WordConnect Bangla</span>
            </div>
            <p className="text-slate-400 max-w-sm mb-8 leading-relaxed">
              The #1 Bengali word puzzle game designed to educate and entertain. Join our community and level up your vocabulary skills.
            </p>
            <div className="flex gap-4">
              <button className="p-3 glass rounded-xl hover:bg-white/10 transition-all"><Camera className="w-5 h-5" /></button>
              <button className="p-3 glass rounded-xl hover:bg-white/10 transition-all"><Send className="w-5 h-5" /></button>
              <button className="p-3 glass rounded-xl hover:bg-white/10 transition-all"><Share2 className="w-5 h-5" /></button>
            </div>
          </div>
          <div>
            <h4 className="font-black mb-6 uppercase tracking-widest text-xs text-cyan-400">Quick Links</h4>
            <ul className="space-y-4 text-slate-400 font-bold text-sm">
              <li><a href="#" className="hover:text-white transition-colors">Home</a></li>
              <li><a href="#features" className="hover:text-white transition-colors">Features</a></li>
              <li><a href="#how-to-play" className="hover:text-white transition-colors">How to Play</a></li>
              <li><a href="#download" className="hover:text-white transition-colors">Download</a></li>
            </ul>
          </div>
          <div>
            <h4 className="font-black mb-6 uppercase tracking-widest text-xs text-cyan-400">Contact & Legal</h4>
            <ul className="space-y-4 text-slate-400 font-bold text-sm">
              <li><Link to="/privacy" className="hover:text-white transition-colors">Privacy Policy</Link></li>
              <li><Link to="/terms" className="hover:text-white transition-colors">Terms of Service</Link></li>
              <li><Link to="/help" className="hover:text-white transition-colors">Help Center</Link></li>
              <li><Link to="/contact" className="hover:text-white transition-colors">Contact Us</Link></li>
            </ul>
          </div>
        </div>
        <div className="max-w-7xl mx-auto pt-8 border-t border-white/5 flex flex-col md:flex-row justify-between items-center gap-4 text-slate-500 text-xs font-bold uppercase tracking-widest">
          <p>© 2026 WORDCONNECT BANGLA. ALL RIGHTS RESERVED.</p>
          <div className="flex gap-8">
            <span>Designed with ❤️ by Antigravity</span>
          </div>
        </div>
      </footer>
    </div>
  );
}

function FAQItem({ question, answer }: { question: string, answer: string }) {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <div className="glass rounded-2xl border-white/5 overflow-hidden">
      <button 
        onClick={() => setIsOpen(!isOpen)}
        className="w-full p-6 text-left flex justify-between items-center hover:bg-white/5 transition-colors"
      >
        <span className="font-bold text-lg">{question}</span>
        <Zap className={`w-4 h-4 transition-transform ${isOpen ? 'rotate-180 fill-cyan-400 text-cyan-400' : 'text-slate-500'}`} />
      </button>
      <motion.div 
        initial={false}
        animate={{ height: isOpen ? 'auto' : 0, opacity: isOpen ? 1 : 0 }}
        className="overflow-hidden"
      >
        <p className="p-6 pt-0 text-slate-400 leading-relaxed border-t border-white/5">
          {answer}
        </p>
      </motion.div>
    </div>
  );
}

