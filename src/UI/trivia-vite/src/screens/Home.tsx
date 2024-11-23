import { Button } from "@/components/ui/button"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/components/ui/tabs"
import { useGameStore } from '../store/GameStore'
 
export function Home() {

  const newOrJoinState = useGameStore(s => s.newOrJoinState);
  const checkGame = useGameStore(s => s.checkGame);
  return (
    <>
    <h1>Fast Trivia</h1>
    <Tabs defaultValue="join" className="w-[400px]" onValueChange={(v) => checkGame(newOrJoinState.name, v == 'new-game')}>
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="join">Join Game</TabsTrigger>
        <TabsTrigger value="new-game">New Game</TabsTrigger>
      </TabsList>
      <TabsContent value="join">
        <Card>
          <CardHeader>
            <CardTitle>Join Game</CardTitle>
            <CardDescription>
              Enter Game Code and your details to join. <br/>
              Email address is not shared - for gravitar pic only
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
          <div className="space-y-1">
              <Label htmlFor="code">Game Code</Label>
              <Input id="code" value={newOrJoinState.name} onChange={(e) => checkGame(e.target.value, false)} />
              <Label htmlFor="name">{newOrJoinState.error}</Label>
            </div>
            <div className="space-y-1">
              <Label htmlFor="name">Display Name</Label>
              <Input id="name" defaultValue="" />
            </div>
            <div className="space-y-1">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" defaultValue="" />
            </div>
          </CardContent>
          <CardFooter>
            <Button disabled={!newOrJoinState.isAvailable}>Join Game</Button>
          </CardFooter>
        </Card>
      </TabsContent>
      <TabsContent value="new-game">
        <Card>
          <CardHeader>
            <CardTitle>New Game</CardTitle>
            <CardDescription>
             Give your game a name or code. Other users can use this to join your game
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="space-y-1">
              <Label htmlFor="new-game-name">Game Name</Label>
              <Input id="new-game-name" value={newOrJoinState.name} onChange={(e) => checkGame(e.target.value, true)}  /> {/* */}
            </div>      
          </CardContent>
          <CardFooter>
            <Button disabled={!newOrJoinState.isAvailable}>Create Game</Button>
          </CardFooter>
        </Card>
      </TabsContent>
    </Tabs>
    <div><pre>{JSON.stringify(newOrJoinState, null, 2) }</pre></div>
    </>
    
  )
}
